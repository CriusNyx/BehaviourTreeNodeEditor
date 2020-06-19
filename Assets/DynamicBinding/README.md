# Method Binding

A method binding is a data structure which is used to serialize and dynamically invoke methods on gameobjects. It allows the development of editors that dynamically bind to code at runtime.

## Method Binding Arguments

Method binding arguments are used to bind arguments to mathod calls dynamically (by name) at runtime. They have different forms, which pull data from different sources at runtime.

### Static Method Binding Argument

Static arguments are defined in the editor as literal objects, and are passed to methods at runtime.

### Memory Method Binding Argument

Memory arguments get objects from the memory data structure passed in which are mapped to enums. How this mapping works depends on the nature of the memory data structure.

For AI, memory arguments are used to get data from the AI's memory, squad's AI memory, or global AI memory.

### Argument Method Binding Argument

Argument arguments (TODO: seek better name) get objects from the memory data structure passed in which are mapped to strings. This type of argument can have options passed in durring validation, which limits the selection to the options presented in the editor

For AI, Argument arguments are used to get arguments from the parameters for a particular AI tree.

### Params Method Binding Argument

Params arguments are used to map "params" parameters in method calls. Params options for a method can be specified using the ParamsDataSourceAttribute

``` c#
// This example class shows an example class with a bindable method and params
// argument
public class ExampleClass{

    // This method has a params arguments and
    // takes in two other arguments, t and u
    [MyMethodBindingAttribute]
    // The params data source attribute tells the runtime where to argument
    // information (types and names of valid arguments) for the params
    [ParamsDataSource(typeof(ExampleClass), nameof(SomeParamsDataSourceMethod))]
    void SomeMethodWithParams(
        T t, 
        U u, 
        params (Type argType, string argName)[] parameters)
    {

    }

    // This method returns valid parameters for the previous method.
    // Notice that t and u are passed in. 
    // These will match the t and u specified for the previous method.
    // This allows you to make params depend on the values of t and u.
    static IEnumerable<(Type argumentType, string argumentName)> 
    SomeParamsDataSourceMethod(
        T t, 
        U u)
    {
        yield return (T1, arg1);
        yield return (T2, arg2);
    }
}
```

For AI, params can be used for any method call, but are used mostly for calling other ai trees as subroutines.