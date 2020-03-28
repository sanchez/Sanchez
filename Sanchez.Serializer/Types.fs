namespace Sanchez.Serializer.Core.Types

type Symbol =
    | UnionSymbol of (string*Symbol)
    | ArraySymbol of Symbol list
    | ObjectSymbol of Map<string, Symbol>
    | NumberSymbol of double
    | StringSymbol of string
    | BooleanSymbol of bool
    | EmptySymbol of unit

type SerializationErrors =
    | FailedToParseType of string
    
type ConverterResult = Result<Symbol, SerializationErrors>