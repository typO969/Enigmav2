''' <summary>
''' Simulates a complete Enigma machine with rotors and reflector.
''' </summary>
Public Class EnigmaMachine
	Public rotors() As Rotor
	Public reflector As String ' e.g. "YRUHQSLDPXNGOKMIEBFZCWVJAT"

	''' <summary>
	''' Initializes a new instance of the EnigmaMachine class with specified rotors and reflector wiring.
	''' </summary>
	''' <param name="rotors">Array of Rotor objects.</param>
	''' <param name="reflector">Reflector wiring as a 26-character string.</param>
	Public Sub New(rotors() As Rotor, reflector As String)
		' Initialises a new Engigma Machine class instance.
		' Instance uses supplied array of Rotors and a dictionary containing the mappings used for the reflector.

		Me.rotors = rotors
		Me.reflector = reflector
	End Sub

	''' <summary>
	''' Rotates the rotors according to Enigma stepping rules, including double-stepping.
	''' </summary>
	Public Sub RotateRotors()
		' Rotates the rotors according to the original Enigma machine mechanics:
		' 1. Right rotor (last in array) always rotates with each keystroke
		' 2. Middle rotor rotates when right rotor hits its notch
		' 3. Left rotor rotates when middle rotor hits its notch
		' 4. "Double stepping anomaly": Middle rotor will step twice when it reaches its own notch (once due to normal stepping and once when it causes the left rotor to step)

		' Get the current position character for each rotor before rotation
		Dim rightNotched As Boolean = False
		Dim middleNotched As Boolean = False

                ' Check if rotors are at notch positions (before rotating)
                Dim rotorCount As Integer = Rotors.Length

                If rotorCount >= 2 Then
                        rightNotched = Rotors(rotorCount - 1).notches.Contains(ChrW(Asc("A") + Rotors(rotorCount - 1).offset))
                End If

                If rotorCount >= 3 Then
                        middleNotched = Rotors(rotorCount - 2).notches.Contains(ChrW(Asc("A") + Rotors(rotorCount - 2).offset))
                End If

                ' Step according to Enigma rules:
                ' Determine rotor indices relative to the right-most rotor
                Dim rightIndex As Integer = rotorCount - 1
                Dim middleIndex As Integer = rotorCount - 2
                Dim leftIndex As Integer = rotorCount - 3

                ' Step 1: If middle rotor is at notch, both middle and left rotors step
                If middleNotched And rotorCount >= 3 Then
                        Rotors(leftIndex).Rotate() ' Left rotor steps
                        Rotors(middleIndex).Rotate() ' Middle rotor steps
                End If

                ' Step 2: If right rotor is at notch, middle rotor steps
                ' (Note: middle rotor can step twice in one operation - the double stepping)
                If rightNotched And rotorCount >= 2 Then
                        Rotors(middleIndex).Rotate() ' Middle rotor steps
                End If

                ' Step 3: Right rotor always steps
                Rotors(rightIndex).Rotate() ' Right-most rotor always rotates
	End Sub

	''' <summary>
	''' Encrypts a single character, passing it through all rotors, the reflector, and back.
	''' </summary>
	''' <param name="plainChar">The character to encrypt.</param>
	''' <returns>The encrypted character.</returns>
	Public Function EncryptChar(plainChar As Char) As Char
		' Encrypts a character by encrypting it with the first rotor and then passing the result into the next rotor.
		' Calls EnigmaMachine.RotateRotors() before fully encrypting a character

		Dim cipherChar As Char = plainChar

		' First rotate the rotors so the next character uses a new alphabet.
		RotateRotors()

		' Pass the character through the rotors from right to left.
		For Each Rot As Rotor In Rotors.Reverse()
			cipherChar = Rot.StandardEncrypt(cipherChar)
		Next

		' Reflect the character using the reflector wiring string
		If Char.IsLetter(cipherChar) Then
			Dim idx As Integer = Asc(Char.ToUpper(cipherChar)) - Asc("A")
			If idx < 0 OrElse idx > 25 Then
				Throw New translationException("Reflector could not map " & cipherChar)
			End If
			cipherChar = Reflector(idx)
		End If

		' Pass the character through the rotors from left to right.
		For Each Rot As Rotor In Rotors
			cipherChar = Rot.ReflectEncrypt(cipherChar)
		Next

		' Introduce glitch only in REAL mode
		If Not Module1.easyMode Then
			Dim rand As New Random()
			For Each Rot As Rotor In Rotors
				If Rot.hasContactIssues AndAlso rand.Next(10000) < 1 Then ' ~0.01% chance
					Dim shift = If(rand.Next(2) = 0, -1, 1)
					cipherChar = ChrW(((Asc(cipherChar) - Asc("A") + shift + 26) Mod 26) + Asc("A"))
					Exit For
				End If
			Next
		End If

		Return cipherChar
	End Function

	''' <summary>
	''' Encrypts a string of text, formatting output in groups if specified.
	''' </summary>
	''' <param name="plainText">The text to encrypt.</param>
	''' <param name="formatInGroups">Whether to format output in 5-letter groups.</param>
	''' <returns>The encrypted text.</returns>
	Public Function EncryptText(plainText As String, Optional formatInGroups As Boolean = True) As String
		If String.IsNullOrEmpty(plainText) Then
			Return String.Empty
		End If

		' Convert to uppercase and remove all non-alphabetic characters
		Dim filteredText As String = ""
		For Each c As Char In plainText.ToUpper()
			If c >= "A"c AndAlso c <= "Z"c Then
				filteredText += c
			End If
		Next

		' Encrypt the filtered text
		Dim cipherText As String = ""
		For Each plainChar As Char In filteredText
			cipherText += EncryptChar(plainChar)
		Next

		' Format in groups of 5 letters if requested
		If formatInGroups Then
			Dim formattedText As String = ""
			For i As Integer = 0 To cipherText.Length - 1
				formattedText += cipherText(i)
				If (i + 1) Mod 5 = 0 AndAlso i < cipherText.Length - 1 Then
					formattedText += " "
				End If
			Next
			Return formattedText
		Else
			Return cipherText
		End If
	End Function

End Class
