using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace DIScript.Tests
{
    public interface IFoo
    {

    }

    public class Foo
    {

    }

    public class BasicTests
    {
        private DIScriptParser Parse(string content)
        {
            var str = new AntlrInputStream(content);
            var lexer = new DIScriptLexer(str);
            var tokens = new CommonTokenStream(lexer);
            return new DIScriptParser(tokens);
        }

        internal class ScriptRunnerVisitor : DIScriptBaseVisitor<Dictionary<string, ServiceCollection>>
        {
            private Dictionary<string, ServiceCollection> _dictionary = new Dictionary<string, ServiceCollection>(StringComparer.OrdinalIgnoreCase);
            private ServiceCollection _current;
            public ScriptRunnerVisitor()
            {
            }
            public override Dictionary<string, ServiceCollection> Visit(IParseTree tree)
            {
                base.Visit(tree);
                return _dictionary;
            }
            public override Dictionary<string, ServiceCollection> VisitRegistrations([NotNull] DIScriptParser.RegistrationsContext context)
            {
                var name = context.name?.GetText() ?? "main";
                if(!_dictionary.TryGetValue(name, out _current))
                {
                    _current = _dictionary[name] = new ServiceCollection();
                }
                else
                {
                    throw new Exception($"The registration group name '{name}' (at Line {context.Start.Line}, Position {context.Start.StartIndex}) is defined more than once");
                }

                var result = base.VisitRegistrations(context);
                _current = null;
                return result;
            }

            public override Dictionary<string, ServiceCollection> VisitRegistration([NotNull] DIScriptParser.RegistrationContext context)
            {
                var lifetime = Enum.Parse<ServiceLifetime>(context.lifetime().GetText() ?? nameof(ServiceLifetime.Transient));
                _current.Add(new ServiceDescriptor(typeof(BasicTests).Assembly.GetType(context.service.GetText()), typeof(BasicTests).Assembly.GetType(context.implementation.GetText()), lifetime));
                return base.VisitRegistration(context);
            }
        }

        [Fact]
        public void ShouldDoSomething()
        {
            var parser = Parse(@"
{
  DIScript.Tests.Foo as DIScript.Tests.IFoo Singleton;
}
");
            var context = parser.scriptFile();
            var result = new ScriptRunnerVisitor().Visit(context);
            Assert.Single(result);
            Assert.True(result.ContainsKey("main"));
            var registration = Assert.Single(result["main"]);
            Assert.Equal(typeof(IFoo), registration.ImplementationType);
            Assert.Equal(typeof(Foo), registration.ServiceType);
            Assert.Equal(ServiceLifetime.Singleton, registration.Lifetime);
        }
    }
}
