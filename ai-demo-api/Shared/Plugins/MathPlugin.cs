using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace Shared.Plugins;

public class MathPlugin : IPlugin
{
    [KernelFunction("add")]
    [Description("Adds two numbers.")]
    public string Add(
        [Description("The first number.")] double number1,
        [Description("The second number.")] double number2)
    {
        return $"{number1 + number2}";
    }

    [KernelFunction("subtract")]
    [Description("Subtracts the second number from the first.")]
    public string Subtract(
        [Description("The first number.")] double number1,
        [Description("The second number.")] double number2)
    {
        return $"{number1 - number2}";
    }

    [KernelFunction("multiply")]
    [Description("Multiplies two numbers.")]
    public string Multiply(
        [Description("The first number.")] double number1,
        [Description("The second number.")] double number2)
    {
        return $"{number1 * number2}";
    }

    [KernelFunction("divide")]
    [Description("Divides the first number by the second.")]
    public string Divide(
        [Description("The numerator.")] double numerator,
        [Description("The denominator. Cannot be zero.")] double denominator)
    {
        if (denominator == 0)
        {
            return "Division by zero is not allowed.";
        }
        return $"{numerator / denominator}";
    }

    [KernelFunction("power")]
    [Description("Raises the base to the power of the exponent.")]
    public string Power(
        [Description("The base number.")] double baseNumber,
        [Description("The exponent.")] double exponent)
    {
        return $"{Math.Pow(baseNumber, exponent)}";
    }

    [KernelFunction("square_root")]
    [Description("Returns the square root of a number.")]
    public string SquareRoot(
        [Description("The number. Cannot be negative.")] double number)
    {
        if (number < 0)
        {
            return "Square root of a negative number is not allowed.";
        }
        return $"{Math.Sqrt(number)}";
    }
}
