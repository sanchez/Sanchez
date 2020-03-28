namespace Sanchez.Serializer.Core.Types

type Symbol =
    | UnionSymbol of (string*Symbol)
    | ArraySymbol of Symbol list
    | ObjectSymbol of Map<string, Symbol>
    | NumberSymbol of double
    | StringSymbol of string
    | BooleanSymbol of bool
    | EmptySymbol of unit
    
module Symbol =
    let toLabel (s: Symbol) =
        match s with
        | UnionSymbol _ -> "Union"
        | ArraySymbol _ -> "Array"
        | ObjectSymbol _ -> "Object"
        | NumberSymbol _ -> "Number"
        | StringSymbol _ -> "String"
        | BooleanSymbol _ -> "Boolean"
        | EmptySymbol _ -> "Empty"

type SerializationErrors =
    | FailedToParseType of string
    | UnsupportedType of string
    | MismatchedTypes of string*string // (provided type, symbol type)
    | MissingObjectKey of string*string // (key, name of type)
    | MissingUnionCase of string*string // (case, name of type)
    
type ConverterResult = Result<Symbol, SerializationErrors>
type ObjectConverterResult = Result<obj, SerializationErrors>