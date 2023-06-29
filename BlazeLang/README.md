# Blaze Language
Foundationally created via reading / studying the book [**Crafting Interpreters** by **Robert Nystrom**](https://craftinginterpreters.com/).

Will be extended to be switched into the Blaze Language.

# Language Breakdown
All changes should be treated as they can change at any time. This README will be updated for every version release. A full file breakdown is located in `lang.blaze`.

## Comments
```js
// Single-Line Comment
```

## Data Types
- Numbers can be integers or doubles.
```go
45 45.43            // Numbers
"foo"               // Strings
nil                 // Null Value
true false          // Booleans
[1, "two", true]    // Lists
```

## Arithmetic Operators
```go
+   // Addition
-   // Subtraction
*   // Multiplication
/   // Division
```

## Comparison Operators
```cs
<       // Less Than
<=      // Less Than Equal To
>       // Greater Than
>=      // Greater Than Equal To
==      // Equal To
and     // And Comparison (&&)
or      // Or Comparison (||)
```

## Variable Declaration
```cs
var foo;            // Un-initialized Variable
var bar = "foo";    // Initialized Variable
```

## Variable Assignment
```cs
foo = 69.420;       // Assigning a Variable
```

## Lists
```js
var list = [1, "apple", true, nil];     // Lists can store multiple data-types
list[0];                                // Access a list item by index
list[-1];                               // Use negative access modifier for end of list
```

## User-Defined Functions
```kotlin
// Function Declaration
fun add(a, b) {
    var result = a + b;

    return result;
}

// Function Call
add(5, 5);

// Nested Functions
fun parent() {
    var test = 1;

    fun child() {
        return test;
    }

    return child;   // Returns the function object, when called it will return '1';
}
```

## Conditonal Statements
```cs
// If - ElseIf - Else Statements
if (10 > 10) {
    var a = 10;
} else if (10 == 10) {
    var a = 11;
} else {
    var a = 0;
}
```

## Loops
```c
// Classic For Loop
for (var i = 0; i < 10; i = i + 1) {

    // Infinite For Loop
    for (;;) {

    }

}

// While Loop
while (true) {

}
```

## Classes (OOP)
```js
class Parent {
    // Constructor
    init() {
        // Class Variables
        this.bar = true;
    }

    // User-Defined Methods
    method() {
        return 420;
    }
}

// Inheritance
class Child < Parent {
    // Constructor is optional

    callParentMethod() {
        // 'super' keyword to reference parent class methods
        return super.method();
    }
}
```

## Native Functions
```js
// Core Module
print "value";      // Prints a value to the console
clock();            // Gets the current time in seconds

// List Functions
append(list, value)             // Appends a value to a list
remove(list, index)             // Removes a value at a specified index of the list
slice(list, start, end, step)   // Slices the current list and returns a new one.
```
