using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static VectorExtensions;

public class VectorConversionTests
{
    [Test]
    [TestCaseSource(nameof(VectorComponentTestData))]
    public void VectorComponentParsesCorrectly(string s, VectorComponent[] expected)
    {
        VectorComponent[] actual = VectorComponent.Parse(s, expected.Length);
        Assert.That(actual, Is.EquivalentTo(expected));
    }

    private static IEnumerable<object[]> VectorComponentTestData()
    {
        yield return new object[] { "", new[] { new VectorComponent(false, '0'), new VectorComponent(false, '0'), new VectorComponent(false, '0') } };
        yield return new object[] { "000", new[] { new VectorComponent(false, '0'), new VectorComponent(false, '0'), new VectorComponent(false, '0') } };
        yield return new object[] { "xyz", new[] { new VectorComponent(false, 'x'), new VectorComponent(false, 'y'), new VectorComponent(false, 'z') } };
        yield return new object[] { "-x-y-z", new[] { new VectorComponent(true, 'x'), new VectorComponent(true, 'y'), new VectorComponent(true, 'z') } };
        yield return new object[] { "-x", new[] { new VectorComponent(true, 'x'), new VectorComponent(false, '0'), new VectorComponent(false, '0') } };
    }
}