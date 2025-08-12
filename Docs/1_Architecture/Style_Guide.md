好的，这是转换后的 Markdown 格式。

# C\# style guide

Having well-defined and consistent coding conventions is important for every project, and Godot
is no exception to this rule.

This page contains a coding style guide, which is followed by developers of and contributors to Godot
itself. As such, it is mainly intended for those who want to contribute to the project, but since
the conventions and guidelines mentioned in this article are those most widely adopted by the users
of the language, we encourage you to do the same, especially if you do not have such a guide yet.

> **Note:** This article is by no means an exhaustive guide on how to follow the standard coding
> conventions or best practices. If you feel unsure of an aspect which is not covered here,
> please refer to more comprehensive documentation, such as
> [C\# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/inside-a-program/coding-conventions) or
> [Framework Design Guidelines](https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/naming-guidelines).

## Language specification

Godot currently uses **C\# version 12.0** in its engine and example source code,
as this is the version supported by .NET 8.0 (the current baseline requirement).
So, before we move to a newer version, care must be taken to avoid mixing
language features only available in C\# 13.0 or later.

For detailed information on C\# features in different versions, please see
[What's New in C\#](https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/).

## Formatting

### General guidelines

  * Use line feed (**LF**) characters to break lines, not CRLF or CR.
  * Use one line feed character at the end of each file, except for `csproj` files.
  * Use **UTF-8** encoding without a [byte order mark](https://en.wikipedia.org/wiki/Byte_order_mark).
  * Use **4 spaces** instead of tabs for indentation (which is referred to as "soft tabs").
  * Consider breaking a line into several if it's longer than 100 characters.

### Line breaks and blank lines

For a general indentation rule, follow [the "Allman Style"](https://en.wikipedia.org/wiki/Indentation_style#Allman_style)
which recommends placing the brace associated with a control statement on the next line, indented to
the same level:

```csharp
// Use this style:
if (x > 0)
{
    DoSomething();
}

// NOT this:
if (x > 0) {
    DoSomething();
}
```

However, you may choose to omit line breaks inside brackets:

  * For simple property accessors.
  * For simple object, array, or collection initializers.
  * For abstract auto property, indexer, or event declarations.

<!-- end list -->

```csharp
// You may put the brackets in a single line in following cases:
public interface MyInterface
{
    int MyProperty { get; set; }
}

public class MyClass : ParentClass
{
    public int Value
    {
        get { return 0; }
        set
        {
            ArrayValue = new [] {value};
        }
    }
}
```

Insert a blank line:

  * After a list of `using` statements.
  * Between method, properties, and inner type declarations.
  * At the end of each file.

Field and constant declarations can be grouped together according to relevance. In that case, consider
inserting a blank line between the groups for easier reading.

Avoid inserting a blank line:

  * After `{`, the opening brace.
  * Before `}`, the closing brace.
  * After a comment block or a single-line comment.
  * Adjacent to another blank line.

<!-- end list -->

```csharp
using System;
using Godot;

public class MyClass
{
    public enum MyEnum
    {
        Value,
        AnotherValue
    }

    public const int SomeConstant = 1;
    public const int AnotherConstant = 2;

    private Vector3 _x;
    private Vector3 _y;

    private float _width;
    private float _height;

    public int MyProperty { get; set; }

    public void MyMethod()
    {
        // Some comment.
        AnotherMethod();
    }

    public void AnotherMethod()
    {
    }
}
```

### Using spaces

Insert a space:

  * Around a binary and ternary operator.
  * Between an opening parenthesis and `if`, `for`, `foreach`, `catch`, `while`, `lock` or `using` keywords.
  * Before and within a single line accessor block.
  * Between accessors in a single line accessor block.
  * After a comma which is not at the end of a line.
  * After a semicolon in a `for` statement.
  * After a colon in a single line `case` statement.
  * Around a colon in a type declaration.
  * Around a lambda arrow.
  * After a single-line comment symbol (`//`), and before it if used at the end of a line.
  * After the opening brace, and before the closing brace in a single line initializer.

Do not use a space:

  * After type cast parentheses.

The following example shows a proper use of spaces, according to some of the above mentioned conventions:

```csharp
public class MyClass<A, B> : Parent<A, B>
{
    public float MyProperty { get; set; }

    public float AnotherProperty
    {
        get { return MyProperty; }
    }

    public void MyMethod()
    {
        int[] values = { 1, 2, 3, 4 };
        int sum = 0;

        // Single line comment.
        for (int i = 0; i < values.Length; i++)
        {
            switch (i)
            {
                case 3: return;
                default:
                    sum += i > 2 ? 0 : 1;
                    break;
            }
        }

        i += (int)MyProperty; // No space after a type cast.
    }
}
```

## Naming conventions

Use **PascalCase** for all namespaces, type names and member level identifiers (i.e. methods, properties,
constants, events), except for private fields:

```csharp
namespace ExampleProject
{
    public class PlayerCharacter
    {
        public const float DefaultSpeed = 10f;

        public float CurrentSpeed { get; set; }

        protected int HitPoints;

        private void CalculateWeaponDamage()
        {
        }
    }
}
```

Use **camelCase** for all other identifiers (i.e. local variables, method arguments), and use
an underscore (`_`) as a prefix for private fields (but not for methods or properties, as explained above):

```csharp
private Vector3 _aimingAt; // Use an `_` prefix for private fields.

private void Attack(float attackStrength)
{
    Enemy targetFound = FindTarget(_aimingAt);

    targetFound?.Hit(attackStrength);
}
```

There's an exception with acronyms which consist of two letters, like `UI`, which should be written in
uppercase letters where PascalCase would be expected, and in lowercase letters otherwise.

Note that `id` is **not** an acronym, so it should be treated as a normal identifier:

```csharp
public string Id { get; }

public UIManager UI
{
    get { return uiManager; }
}
```

It is generally discouraged to use a type name as a prefix of an identifier, like `string strText`
or `float fPower`, for example. An exception is made, however, for interfaces, which
**should**, in fact, have an uppercase letter `I` prefixed to their names, like `IInventoryHolder` or `IDamageable`.

Lastly, consider choosing descriptive names and do not try to shorten them too much if it affects
readability.

For instance, if you want to write code to find a nearby enemy and hit it with a weapon, prefer:

```csharp
FindNearbyEnemy()?.Damage(weaponDamage);
```

Rather than:

```csharp
FindNode()?.Change(wpnDmg);
```

## Member variables

Don't declare member variables if they are only used locally in a method, as it
makes the code more difficult to follow. Instead, declare them as local
variables in the method's body.

## Local variables

Declare local variables as close as possible to their first use. This makes it
easier to follow the code, without having to scroll too much to find where the
variable was declared.

## Implicitly typed local variables

Consider using implicitly typing (`var`) for declaration of a local variable, but do so
**only when the type is evident** from the right side of the assignment:

```csharp
// You can use `var` for these cases:

var direction = new Vector2(1, 0);

var value = (int)speed;

var text = "Some value";

for (var i = 0; i < 10; i++)
{
}

// But not for these:

var value = GetValue();

var velocity = direction * 1.5;

// It's generally a better idea to use explicit typing for numeric values, especially with
// the existence of the `real_t` alias in Godot, which can either be double or float
// depending on the build configuration.

var value = 1.5;
```

## Other considerations

  * Use explicit access modifiers.
  * Use properties instead of non-private fields.
  * Use modifiers in this order:
    `public`/`protected`/`private`/`internal`/`virtual`/`override`/`abstract`/`new`/`static`/`readonly`.
  * Avoid using fully-qualified names or `this.` prefix for members when it's not necessary.
  * Remove unused `using` statements and unnecessary parentheses.
  * Consider omitting the default initial value for a type.
  * Consider using null-conditional operators or type initializers to make the code more compact.
  * Use safe cast when there is a possibility of the value being a different type, and use direct cast otherwise

## Testing Conventions

A consistent and readable testing style is as important as the implementation style. Our testing philosophy prioritizes clarity, expressiveness, and reliability.

### Assertion Library: `FluentAssertions`

All unit and integration tests **must** use the `FluentAssertions` library for assertions. It provides a more readable, expressive, and less error-prone way to state expectations compared to traditional `Assert` statements.

For testing types from the `LanguageExt` library (e.g., `Fin<T>`, `Option<T>`), tests **must** also use the `FluentAssertions.LanguageExt` companion library.

**Example (Good Practice):**
```csharp
// Asserting a simple value
result.Should().Be(5, "because the calculation should yield 5");

// Asserting a LanguageExt Fin<T> type
handlerResult.Should().BeSuccess("because the command was valid");
handlerResult.Should().BeSuccessWithValue(expectedDto);
```

**Example (Bad Practice - To Avoid):**
```csharp
// Do NOT use traditional Assert statements
Assert.Equal(5, result);
Assert.True(handlerResult.IsSucc);
```

### Handling Expected Exceptions

When testing that a method correctly throws an exception under specific circumstances, tests **must not** use `try-catch` blocks. This is considered an anti-pattern as it makes the test's intent less clear and can swallow unexpected exceptions.

Instead, tests **must** use the `Action` delegate and the `Should().Throw<TException>()` extension method from `FluentAssertions`.

**Example (Good Practice):**
```csharp
[Fact]
public void MyService_Constructor_Should_Throw_When_Dependency_Is_Null()
{
    // Arrange
    ILogger nullLogger = null;

    // Act
    // Wrap the code that is expected to throw in an Action
    Action act = () => new MyService(nullLogger);

    // Assert
    // This clearly states the expectation and fails if the wrong exception (or no exception) is thrown.
    act.Should().Throw<ArgumentNullException>()
       .WithParameterName("logger"); // Optionally assert on exception details
}
```

**Example (Bad Practice - To Avoid):**
```csharp
[Fact]
public void MyService_Constructor_Should_Throw_When_Dependency_Is_Null()
{
    // Do NOT use try-catch in tests
    try
    {
        var service = new MyService(null);
        Assert.Fail("Expected an exception but none was thrown.");
    }
    catch (ArgumentNullException)
    {
        // This passes, but it's verbose and less safe.
    }
}
```