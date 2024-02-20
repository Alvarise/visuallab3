using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive;

namespace lab3.ViewModels;

public class MainViewModel : ViewModelBase
{
    private string _expression = "0";
    private string _answer = "";
    private bool comma = false;
    private bool function = false;
    private char operation = '\0';

    public string expression
    {
        get => _expression;
        set => this.RaiseAndSetIfChanged(ref _expression, value);
    }

    public string answer
    {
        get => _answer;
        set => this.RaiseAndSetIfChanged(ref _answer, value);
    }

    private Dictionary<string, Func<double, double>> functions = new Dictionary<string, Func<double, double>>
    {
        { "sin", Math.Sin },
        { "cos", Math.Cos },
        { "tg", Math.Tan },
        { "floor", Math.Floor },
        { "ceil", Math.Ceiling },
        { "lg", Math.Log10 },
        { "n!", (n) => {
            long result = 1;
            for (int i = 2; i <= n; i++)
                result *= i;
            return result; } },
        { "ln", Math.Log },
    };

    private Dictionary<char, Func<double, double, double>> operations = new Dictionary<char, Func<double, double, double>>
    {
        { '^', Math.Pow },
        { '%', (x, y) => x % y },
        { '/', (x, y) => x / y },
        { '*', (x, y) => x * y },
        { '+', (x, y) => x + y },
        { '-', (x, y) => x - y }
    };

    public ReactiveCommand<string, Unit> ChangeStringCommand { get; }

    private void HandleEvent(string text)
    {
        if (functions.ContainsKey(text) && operation == '\0')
        {
            if (answer != "")
                expression = answer;

            answer = functions[text](double.Parse(expression)).ToString();

            if (text == "n!")
                expression += '!';
            else
                expression = text + '(' + expression + ')';

            function = true;
        }

        if (operations.ContainsKey(text[0]) && operation == '\0')
        {
            if (function)
            {
                expression = answer;
                function = false;
            }

            expression += text;
            operation = text[0];
        }

        if (char.IsDigit(text[0]) && !function)
        {
            if (expression == "0")
                expression = text;
            else
                expression += text;
        }

        switch (text)
        {
            case "C":
                expression = "0";
                answer = "";
                comma = false;
                function = false;
                operation = '\0';
                break;

            case ",":
                if (!comma && !function)
                {
                    if (!char.IsDigit(expression[expression.Length - 1]))
                        expression += '0' + text;
                    else
                        expression += text;
                    comma = true;
                }
                break;

            case "⌫":
                if (expression.Length < 2)
                {
                    expression = "0";
                    break;
                }

                if (function)
                    break;
                
                expression = expression.Remove(expression.Length - 1);

                if (expression[expression.Length - 1] == ',')
                {
                    expression = expression.Remove(expression.Length - 1);
                    comma = false;
                }

                break;

            case "=":
                if (operation != '\0')
                {
                    answer = operations[operation](double.Parse(expression.Substring(0, expression.LastIndexOf(operation))),
                                               double.Parse(expression.Substring(expression.LastIndexOf(operation) + 1))).ToString();
                    operation = '\0';
                }
                else
                {
                    expression = answer;
                    answer = "";
                }

                if (double.Parse(expression) == Math.Floor(double.Parse(expression)))
                    comma = false;

                break;
        }
    }

    public MainViewModel()
    {
        ChangeStringCommand = ReactiveCommand.Create<string>(text =>
        {
            HandleEvent(text);
        });
    }
}