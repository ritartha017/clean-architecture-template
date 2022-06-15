using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;

namespace CleanArchitecture.Domain.UnitTests.Shared;

public class AutoMoqDataAttribute : AutoDataAttribute
{
    public AutoMoqDataAttribute() : base(() => CreateFixture(null)) { }

    public AutoMoqDataAttribute(Type type, params string[] methodNames) : base(CreateFixtureWithMethod(type, methodNames)) { }

    private static Func<IFixture> CreateFixtureWithMethod(Type type, params string[] methodNames)
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type));

        if (methodNames == null)
            throw new ArgumentNullException(nameof(methodNames));

        var methods = new List<Action<IFixture>>();

        foreach (var methodName in methodNames)
        {
            if (methodName == null)
                throw new NullReferenceException(nameof(methodName));

            var method = (Action<IFixture>)Delegate.CreateDelegate(typeof(Action<IFixture>), type, methodName);

            if (method == null)
                throw new ArgumentException("Method " + methodName + " not found in " + type.Name, methodName);

            methods.Add(method);
        }

        return () => CreateFixture(methods);
    }

    private static IFixture CreateFixture(List<Action<IFixture>>? methods)
    {
        var fixture = new Fixture();
        fixture.Customize(new AutoMoqCustomization());
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        methods?.ForEach(method => method?.Invoke(fixture));
        return fixture;
    }
}
