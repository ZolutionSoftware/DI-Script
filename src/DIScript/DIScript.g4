grammar DIScript;

scriptFile: (
        loadAssembly
    |	using
    |	registrations)+;

// allows loading of specific assemblies which aren't already loaded.
// Could support a globbing pattern here I suppose?
loadAssembly:       'load' path ';';
using:              'using' namespace ';';
registrations:      name=registrationName? LCURL using* registration* RCURL;
registrationName:   ID;
registration:       service=type 'as' implementation=type lifetime?';';

path:               .+?;
namespace:          dottedName;
type:
    dottedName                                  # nonGeneric
    | dottedName '<' numArgs+=','* '>'          # openGeneric
    | dottedName '<' (type (',' type)*)? '>'    # closedGeneric;
lifetime:           ID;
dottedName:         ID ('.' ID)*;

ID:                 (LETTER | '_') IDAny*;
LCURL:              '{';
RCURL:              '}';
BSLASH:             '\\';
FSLASH:             '/';

fragment DIGIT:     [0-9];
fragment LETTER:    [a-zA-Z];
fragment IDAny:     (DIGIT | LETTER | '_');

COMMENT:            '#' ~[\r\n]* -> channel(HIDDEN);
WS:                 [ \r\n\t]+ -> channel(HIDDEN);