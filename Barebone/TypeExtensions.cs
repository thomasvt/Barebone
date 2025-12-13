namespace Barebone
{
    public static class TypeExtensions
    {
        /// <summary>
        /// Writes a type including the generic argument's filename like it occurs in C# code, instead of what .NET shows at runtime.
        /// </summary>
        public static string GetCSharpTypeName(this Type t)
        {
            if (t.DeclaringType != null)
            {
                // it's a nested type
                var parentType = t.DeclaringType;
                if (parentType.IsGenericTypeDefinition) 
                {
                    // parent is an open generic, we must close it.

                    // This turned out to be a lot more complex than expected.
                    //
                    // Explanation:
                    //
                    // ClassA<MyItem>.ClassB will show ClassB as closed generic, but DeclaringType 'ClassA' as open generic,
                    // so both show up as generic but share the same single generic argument. If you add more generic arguments to ClassA and ClassB,
                    // it gets complicated quickly.
                    //
                    // Because we want to show the type arguments as they occurs in C# code, we need to 
                    // make the parent type closed by finding the concrete types in the child's generic arguments.
                    //
                    // It seems you can only recognize shared generic arguments by their generic parameter filename: 'T', 'U' etc. A child class cannot
                    // reuse 'T' if the parent already has a 'T', so that seems to be airtight.

                    var parentGenParams = parentType.GetGenericArguments();
                    var childGenParams = t.GetGenericTypeDefinition().GetGenericArguments(); // eg. [ 'T', 'U' ]
                    var childGenArgs = t.GetGenericArguments(); // eg. [ 'int', 'string' ]
                    var childGenArgsByParamName = childGenParams.Select((p, i) => (Name: p.Name, Type: childGenArgs[i]))
                        .ToDictionary(tpl => tpl.Name, tpl => tpl.Type);

                    var genericArgumentForParent = parentGenParams.Select(p => childGenArgsByParamName[p.Name]).ToArray();

                    parentType = parentType.MakeGenericType(genericArgumentForParent);

                    // Now the generic arguments still show up on the child too, this is redundant. But removing them from the child because
                    // we found out they belong to the parent, is not easy. There could be a mix of parent-shared and child-only parameters.
                    // I'm not sure if this is worth the effort.
                }
                return $"{GetCSharpTypeName(parentType)}.{GetCSharpTypeNameInternal(t)}";
            }
            return GetCSharpTypeNameInternal(t);
        }

        private static string GetCSharpTypeNameInternal(Type t)
        {
            if (t.IsGenericType)
            {
                var outer = t.GetGenericTypeDefinition();
                var outerName = outer.Name;
                if (outerName.Contains('`'))
                    outerName = outerName[..outerName.IndexOf('`')];
                var inner = t.GetGenericArguments();
                return $"{outerName}<{string.Join(", ", inner.Select(i => i.GetCSharpTypeName()))}>";
            }

            return t.Name;
        }
    }
}
