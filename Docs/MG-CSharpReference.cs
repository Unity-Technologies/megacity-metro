// | MICROGAME CODING STANDARD-COMPLIANT REFERENCE C# FILE
// | This is a shortened version of "CODING STANDARD-COMPLIANT REFERENCE C# FILE".
// | You can find the original version here: https://ono.unity3d.com/unity-extra/unity-meta/raw/@/ReferenceSource/CSharp/Assets/CSharpReference.cs
// | If you are not a Unity employee, the original file should be located next to this file.
// | Microgame-specific recommendations are annotated with "MICROGAME".

// |[Usings]
// |    - Located at file scope at the top of the file, never within a namespace.
// |    - Three groups, which are, top to bottom: System, non-System, aliases. Keep each group sorted.
// |    - Strip unused 'usings' except the 'minimally-required set', which is marked with *required below.
// |    - Only use aliases when required by the compiler for disambiguation, and not for hiding rarely-used symbols behind a prefix.
// |    - Always drop explicit namespace qualifications on types when a 'using' can be added (i.e. almost all of the time).
using System;                                                                                       // | Not required, but strongly encouraged
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Win32;                                                                              // | Start of non-System group
using UnityEngine;

using Component = UnityEngine.Component;                                                            // | Start of aliases group
using Debug = UnityEngine.Debug;

// MICROGAME: contrary to the original document, this document covers only
// the conventions used for the Unity namespace which we use for Microgames.
namespace Unity.MicrogameName                                                                        // | Full contents of namespace indented
{
    // |[Everything]
    // |    - Drop redundant access specifiers (leave off 'private' at type scope).
    // |    - Code within the braces always indented one tab stop
    // |    - All opening braces are always on its own line at the same level of indentation as its parent

    // |[Enums & Flags enums]
    // |    - Use a singular type name, and no prefix or suffix
    // |    - Constant names should have no prefix or suffix.
    // |    - Do not specify constant values unless absolutely required (e.g. for version-safe protocols - rare).
    enum WindingOrder
    {
        Clockwise,
        CounterClockwise,
        Charm,
        Singularity,                                                                                // | Trail last element in a list with ','
    }                                                                                               // | Closing brace is on its own line at same level of indentation as parent
                                                                                                    // | Put exactly one blank line between multi-line types
    // |[Flags enums]
    // |    - Use column-aligned bit shift expressions for the constants (instead of 2, 4, 8, etc.)
    [Flags]
    public enum VertexStreams
    {
        Position    = 1 << 0,
        Normal      = 1 << 1,
        Tangent     = 1 << 2,
        Color       = 1 << 3,
        UV          = 1 << 4,
    }

    // |[Classes]
    // |    - Name classes and structs with nouns or noun phrases.
    // |    - No prefix on class names (no 'C' or 'S' etc.).
    class Example
    {
        // |[Fields]
        // |    - Use prefix + PascalCase for non-public field naming.
        // |        - Also prefix static/instance readonly with k_ if the intent is to treat the field as deeply const.
        // |    - Drop redundant initializers (i.e. no '= 0' on the ints, '= null' on ref types, etc.).
        // |    - Never expose public fields which are not const or static readonly. These fields should be published through a property.
        // |    - Use readonly where const isn't possible.
        int m_CurrentCount;                                                                         // | m_ = instance field
        static readonly Vector3 k_DefaultLength = new Vector3(1, 2, 3);                             // | k_ = const/readonly
        const int k_MaxCount = 200;
        static int s_SharedCount;                                                                   // | s_ = static readwrite field

        public const int TotalCount = 123;                                                          // | Public fields and properties are PascalCase with no prefix

        public string DefaultName => Environment.MachineName;                                       // MICROGAME: you can use expression bodies for get-only properties

        [Example]                                                                                   // | Attributes always go on a line separate from what they apply to (unless a parameter), and joining them is encouraged if they are short
        public int CurrentCount                                                                     // | Drop 'Attribute' postfix when applying an attribute
        {
            get { return m_CurrentCount; }                                                          // | Getters are always trivial and do not mutate state (this includes first-run cached results); use a full method if you want to do calculations or caching
            set { m_CurrentCount = value; }                                                         // | For multiline method bodies, the 'get' and 'set' keywords must be on their own line
        }                                                                                           // | Put exactly one blank line between multi-line methods and properties

        public int BetterCurrentCount { get; set; } = 1000;                                         // | MICROGAME: use of auto-properties and auto-property initalizers encouraged

        public string Description => $"shared: {s_SharedCount}\ncurrent: {m_CurrentCount}\n";       // | MICROGAME: prefer use of string interpolation over string.Format() et al.

        // MICROGAME: for clarity, documenting the capitalization conventions of names that contain abbreviations,
        // these follow the FDG: https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/capitalization-conventions
        class XmlHttpRequest {}
        public const string IPAddress = "127.0.0.1";                                                // | A special case is made for two-letter acronyms in which both letters are capitalized

        // |[Events]
        // |    - Do not declare new delegate types. Use Action<...> instead.
        // |    - Do not expose public delegate fields. Use events instead.
        // |    - Include one participle-form verb in the event name (generally ending in -ed or -ing, ex. occurred, loading, started, given)
        // |    - *EventArgs struct parameters are not necessary, but they should be used if the data sent to the event has the possibility of needing to be changed. [FDG Exception]
        public event Action<ThingHappenedEventArgs> ThingHappened;

        // |[Methods]
        // |    - Give methods names that are verbs or verb phrases.
        // |    - Parameter names are camelCase
        public void DoThings(IEnumerable<IThingAgent> thingsToDo, string propertyDescription)       // | For types that are already internal (like class Example), use public instead of internal for members and nested types
        {
            var doneThings = new List<IThingAgent>();                                               // | 'var' required on any 'new' where the type we want is the same as what is being constructed
            var indent = new string(' ', 4);                                                        // | ...even primitive types
                                                                                                    // | When appropriate, separate code blocks by a single empty line
            IList<string> doneDescriptions = new List<string>();                                    // | (This is a case where 'var' not required because the types of the variable vs the ctor are different)

            foreach (var thingToDo in thingsToDo)                                                   // | 'var' required in all foreach
            {
                if (!thingToDo.DoThing(propertyDescription, m_CurrentCount))
                    break;                                                                          // | Braces not required for single statements under if or else, but that single statement must be on its own line

                using (File.CreateText(@"path\to\something.txt"))                                   // | Use @"" style string literal for paths with backslashes and regular expression patterns
                using (new ComputeBuffer(10, 20))                                                   // | Don't use braces for directly nested using's
                {                                                                                   // | Braces required for deepest level of nested using's
                    doneThings.Add(thingToDo);
                }
            }

            foreach (var doneThing in doneThings)                                                   // | Dirty details about allocs at https://q.unity3d.com/questions/1465/when-does-using-foreach-in-c-cause-an-allocation.html
            {                                                                                       // | Braces are required for loops (foreach, for, while, do) as well as 'fixed' and 'lock'
                doneDescriptions.Add(doneThing.operationDescription);
                Debug.Log($"{indent}Doing thing: {doneThing.operationDescription}");
            }

            Debug.Log("System Object is " + typeof(object));                                        // | Always use lowercase `object` for the System.Object class.
            Debug.Log("Unity Object is " + typeof(UnityEngine.Object));                             // | Always use a fully qualified name for Unity's Object type, and never 'Object'

            ThingHappened?.Invoke();                                                                // | MICROGAME: use null conditional operator when applicable.
        }

        public void ControlFlow(string message, object someFoo, WindingOrder windingOrder)          // | Use c# aliases of System types (e.g. object instead of Object, float instead of Single, etc.)
        {
            for (int i = 0; i < k_MaxCount; ++i)                                                    // | Using i and j for trivial local iterators is encouraged
            {
                // all of this is nonsense, and is just meant to demonstrate formatting             // | Place comments about multiple lines of code directly above them, with one empty line above the comment to visually group it with its code
                if ((i % -3) - 1 == 0)                                                              // | Wrap parens around subexpressions is optional but recommended to make operator precedence clear
                {
                    ++m_CurrentCount;
                    s_SharedCount *= (int)k_DefaultLength.x + totalCount;

                    do                                                                              // | 'while', 'do', 'for', 'foreach', 'switch' are always on a separate line from the code block they control
                    {
                        i += s_SharedCount;
                    }
                    while (i < m_CurrentCount);
                }
                else                                                                                // | 'else' always at same indentation level as its 'if'
                {
                    Debug.LogWarning("Skipping over " + i);                                         // | Drop 'ToString()' when not required by compiler
                    break;
                }
            }

            // more nonsense code for demo purposes
            switch (windingOrder)
            {
                case WindingOrder.Clockwise:                                                        // | Case labels indented under switch
                case WindingOrder.CounterClockwise:                                                 // | Braces optional if not needed for scope (but note indentation of braces and contents)
                    if (s_SharedCount == k_MaxCount)                                                // | Constants go on the right in comparisons (do not follow 'yoda' style)
                    {
                        var warningDetails = someFoo.ToString();                                    // | 'var' for the result of assignments is optional (either way, good variable naming is most important)
                        for (var i = 0; i < s_SharedCount; ++i)
                        {
                            Debug.LogWarning("Spinning a " + warningDetails);
                        }
                    }
                    break;                                                                          // | 'break' inside case braces, if any

                case WindingOrder.Charm:
                    Debug.LogWarning("Check quark");                                                // | Indentation is the same, with or without scope braces
                    break;

                case WindingOrder.Singularity:
                {
                    var warningDetails = message;                                                   // | (this pointless variable is here solely to require braces on the case statements and show the required formatting)

                    if (message == Registry.ClassesRoot.ToString())
                    {
                        // Already correct so we don't need to do anything here                     // | Empty blocks should (a) only be used when it helps readability, (b) always use empty braces (never a standalone semicolon), and (c) be commented as to why the empty block is there
                    }
                    else if (m_CurrentCount > 3)
                    {
                        if (s_SharedCount < 10)                                                     // | Braces can only be omitted at the deepest level of nested code
                            Debug.LogWarning($"Singularity! ("{warningDetails}")");
                    }
                    else if (s_SharedCount > 5)                                                     // | 'else if' always on same line together
                    {                                                                               // | MICROGAME either the full if-chain uses braces or not (all statements are single-line statements)
                        throw new IndexOutOfRangeException();
                    }
                    else if ((s_SharedCount > 7 && m_CurrentCount != 0) || message == null)         // | Always wrap subexpressions in parens when peer precedence is close enough to be ambiguous (e.g. && and || are commonly confused)
                    {
                        throw new NotImplementedException();
                    }

                    break;
                }

                default:
                    throw new InvalidOperationException($"What's a {windingOrder}?");
            }
        }

        // |[Parameterized Types]
        // |    - When only a single parameterized type is used, naming it 'T' is acceptable.
        // |    - For more than one parameterized type, use descriptive names prefixed with 'T'.
        // |    - Consider indicating constraints placed on a type parameter in the name of the parameter.
        public static TResult Transmogrify<TResult, TComponent>(                                    // | When wrapping params, do not leave any on line with function name
            TComponent component, Func<TComponent, TResult> converter)                              // | When wrapping, only indent one stop (do not line up with paren)
            where TComponent : Component
        {
            return converter(component);
        }
    }

    // |[Structs]
    // |    - Name classes and structs with nouns or noun phrases.
    // |    - No prefix on class names (no 'C' or 'S' etc.).
    // |    - Structs may be mutable, but consider immutability when appropriate. [FDG Exception]
    struct MethodQuery
    {
        public string Name { get; set; }
        public IEnumerable<Type> ParamTypes { get; set; }
        public Type ReturnType { get; set; }

        public override string ToString()                                                           // | Methods generally are not permitted in structs, with exceptions like this noted in the data-oriented programming guidelines.
        {
            var paramTypeNames = paramTypes                                                         // | Prefer fluent function call syntax over LINQ syntax (i.e. y.Select(x => z) instead of 'from x in y select z')
                .Select(p => p.ToString())                                                          // | Prefer breaking long fluent operator chains into one line per operator
                .Where(p => p.Length > 2)
                .OrderBy(p => p[0])
                .ToArray();

            return $"{ReturnType} {Name}({string.Join(", ", paramTypeNames)})";
        }
    }

    // |[Interfaces]
    // |    - Name interfaces with adjective phrases, or occasionally with nouns or noun phrases.
    // |        - Nouns and noun phrases should be used rarely and they might indicate that the type should be an abstract class, and not an interface.
    // |    - Use 'I' prefix to indicate an interface.
    // |    - Ensure that the names differ only by the 'I' prefix on the interface name when you are defining a class-interface pair, where the class is a standard implementation of the interface.
    public interface IThingAgent
    {
        string OperationDescription { get; }
        float Scale { get; }

        bool DoThing(string propertyDescription, int spinCount);
    }

    // |[EventArgs]
    // |    - Always use structs for EventArgs types, and never extend System.EventArgs [FDG Exception]
    // |    - Make EventArgs structs immutable
    // |    - See the event example above for when to define EventArgs structs.
    struct ThingHappenedEventArgs
    {
        public string ThingThatHappened { get; }

        public ThingHappenedEventArgs(string thingThatHappened)
        {
            ThingThatHappened = thingThatHappened;
        }
    }

    // |[Attributes]
    // |    - Mark up all attributes with an AttributeUsage, as narrow as possible.
    // |    - Postfix attribute class names with "Attribute".
    [AttributeUsage(AttributeTargets.Property)]
    public class ExampleAttribute : Attribute
    {                                                                                               // | Empty class bodies have braces on their own lines
    }

    // |[Exceptions]
    // |    - Postfix exception class names with "Exception".
    // |    - Do not inherit from ApplicationException (see http://stackoverflow.com/a/5685943/14582).
    public class ExampleException : Exception
    {
        public ExampleException() {}
        public ExampleException(string message) : base(message) {}
        public ExampleException(string message, Exception innerException) : base(message, innerException) {}
    }
}
