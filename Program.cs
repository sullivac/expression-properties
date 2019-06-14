using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace expression_properties
{
    class Program
    {
        static void Main(string[] args)
        {
            var type = typeof(MyClass);
            var property = type.GetProperty("Name");

            // object target
            var targetParameterExpression = Expression.Parameter(typeof(object), "target");

            // (target as MyClass)
            var castExpression = Expression.TypeAs(targetParameterExpression, typeof(MyClass));

            // (target as MyClass).Name
            var propertyExpression = Expression.Property(castExpression, property);

            // (target as MyClass).Name as object
            var castResultExpression = Expression.TypeAs(propertyExpression, typeof(object));

            // target => (target as MyClass).Name as object
            var lambda = Expression.Lambda<Func<object, object>>(castResultExpression, false, new[] { targetParameterExpression });

            // Cache this
            // IDictionary<Type, IDictionary<string, Func<object, object>>> getters;
            Func<object, object> getName = lambda.Compile();

            var list = Enumerable.Range(1, 100000).Select(value => new MyClass {Name = value.ToString()} );

            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            Console.WriteLine("Reflection GetValue");
            list.Aggregate(Console.Out, (console, item) => {
                    property.GetValue(item);

                    return console;
                });

            stopwatch.Stop();

            Console.WriteLine($"It took {stopwatch.Elapsed}");

            stopwatch.Reset();
            stopwatch.Start();

            Console.WriteLine("Cached Linq Expression");
            list.Aggregate(Console.Out, (console, item) => {
                    getName(item);

                    return console;
                });

            stopwatch.Stop();

            Console.WriteLine($"It took {stopwatch.Elapsed}");

            stopwatch.Reset();
            stopwatch.Start();

            Console.WriteLine("Uncached Linq Expression");
            list.Aggregate(Console.Out, (console, instance) => {
                Func<object, object> notCached = lambda.Compile();
                notCached(instance);

                return console;
            });

            stopwatch.Stop();

            Console.WriteLine($"It took {stopwatch.Elapsed}");

            stopwatch.Reset();
            stopwatch.Start();

            Console.WriteLine("Direct property access");
            Func<MyClass, string> direct = instance => instance.Name;
            list.Aggregate(Console.Out, (console, instance) => {
                direct(instance);

                return console;
            });

            stopwatch.Stop();

            Console.WriteLine($"It took {stopwatch.Elapsed}");
        }
    }

    class MyClass
    {
        public string Name { get; set; }
    }
}
