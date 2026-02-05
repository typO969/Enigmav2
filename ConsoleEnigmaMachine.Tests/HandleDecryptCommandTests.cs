using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using ConsoleEnigmaMachine;
using NUnit.Framework;

namespace ConsoleEnigmaMachine.Tests;

public class HandleDecryptCommandTests
{
    [Test]
    public void HandleDecryptCommand_WithKnownRotorPositions_PrintsExpectedPlaintext()
    {
        // Arrange: load rotor and reflector definitions from existing XML resources.
        var baseDir = AppContext.BaseDirectory;
        var rotorsPath = Path.Combine(baseDir, "Resources", "rotors.xml");
        var reflectorsPath = Path.Combine(baseDir, "Resources", "reflectors.xml");

        var rotors = LoadRotors(rotorsPath, "I", "II", "III");
        var reflector = LoadReflector(reflectorsPath, "B");

        const string expectedPlaintext = "SECRETMESSAGE";
        var initialOffsets = new[] { 5, 10, 15 };

        var encryptMachine = new EnigmaMachine(CloneRotors(rotors), reflector);
        ApplyOffsets(encryptMachine, initialOffsets);
        var cipherText = encryptMachine.EncryptText(expectedPlaintext, formatInGroups: false);

        var decryptMachine = new EnigmaMachine(CloneRotors(rotors), reflector);
        ApplyOffsets(decryptMachine, initialOffsets);

        var decryptCommand = $"--D {cipherText},{initialOffsets[0]},{initialOffsets[1]},{initialOffsets[2]}";

        var originalEasyMode = Module1.easyMode;
        Module1.easyMode = false;

        var originalOut = Console.Out;
        using var writer = new StringWriter();
        Console.SetOut(writer);

        try
        {
            // Act
            Module1.HandleDecryptCommand(ref decryptMachine, decryptCommand);
        }
        finally
        {
            Console.SetOut(originalOut);
            Module1.easyMode = originalEasyMode;
        }

        // Assert
        var output = writer.ToString();
        StringAssert.Contains($"Decrypted: {expectedPlaintext}", output);
    }

    private static Rotor[] LoadRotors(string rotorsXmlPath, params string[] rotorIds)
    {
        var doc = XDocument.Load(rotorsXmlPath);
        return rotorIds
            .Select(id => doc.Root!
                .Elements("rotor")
                .First(r => string.Equals((string)r.Attribute("id"), id, StringComparison.OrdinalIgnoreCase)))
            .Select(r => new Rotor(
                (string)r.Attribute("wiring")!,
                ((string)r.Attribute("notch")!).ToCharArray(),
                (string)r.Attribute("id")!,
                ParseRingSetting(r)))
            .ToArray();
    }

    private static string LoadReflector(string reflectorsXmlPath, string reflectorId)
    {
        var doc = XDocument.Load(reflectorsXmlPath);
        return (string)doc.Root!
            .Elements("reflector")
            .First(r => string.Equals((string)r.Attribute("id"), reflectorId, StringComparison.OrdinalIgnoreCase))
            .Attribute("wiring")!;
    }

    private static int ParseRingSetting(XElement rotorElement)
    {
        var ring = (string?)rotorElement.Attribute("ring");
        return int.TryParse(ring, out var parsed) ? parsed : 0;
    }

    private static Rotor[] CloneRotors(Rotor[] source) =>
        source.Select(r => new Rotor(r.wiring, r.notches.ToArray(), r.iD, r.ringSetting)).ToArray();

    private static void ApplyOffsets(EnigmaMachine machine, int[] offsets)
    {
        for (var i = 0; i < offsets.Length; i++)
        {
            machine.Rotors[i].offset = offsets[i];
        }
    }
}
