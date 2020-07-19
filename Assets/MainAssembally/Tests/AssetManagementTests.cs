using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

public class AssetManagementTests
{
    [Test]
    public void GetParentDirectoryShouldReturnCorrectResults()
    {
        string input = "a/b/c/d/e";
        string[] expected = new string[] { "a", "a/b", "a/b/c", "a/b/c/d", "a/b/c/d/e" };
        string[] actual = AssetManagement.GetAllParentFolderPaths(input);

        Assert.True(
            expected.SequenceEqual(actual), 
            $"TestFailed {nameof(GetParentDirectoryShouldReturnCorrectResults)}\n" +
            $"Expected {{{string.Join(", ", expected)}}}" +
            $"\nActual {{{string.Join(", ", actual)}}}");
    }
}