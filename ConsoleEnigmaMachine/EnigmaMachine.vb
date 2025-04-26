Public Class EnigmaMachine
    Public Rotors() As Rotor
    Public Reflector As Dictionary(Of Char, Char)

    Public Sub New(ByVal Rotors() As Rotor, ByVal Reflector As Dictionary(Of Char, Char))
        '' Initialises a new Engigma Machine class instance.
        '' Instance uses supplied array of Rotors and a dictionary containing
        '' the mappings used for the reflector.

        Me.Rotors = Rotors
        Me.Reflector = Reflector
    End Sub

	Public Sub RotateRotors()
		'' Rotates the rotors according to the original Enigma machine mechanics:
		'' 1. Right rotor (last in array) always rotates with each keystroke
		'' 2. Middle rotor rotates when right rotor hits its notch
		'' 3. Left rotor rotates when middle rotor hits its notch
		'' 4. "Double stepping anomaly": Middle rotor will step twice when it 
		''    reaches its own notch (once due to normal stepping and once when it
		''    causes the left rotor to step)

		' Get the current position character for each rotor before rotation
		Dim rightNotched As Boolean = False
		Dim middleNotched As Boolean = False

		' Check if rotors are at notch positions (before rotating)
		If UBound(Me.Rotors) >= 2 Then ' We have at least 3 rotors
			' Check if right rotor is at notch position
			rightNotched = Me.Rotors(2).Notches.Contains(Me.Rotors(2).Mappings.Keys.ElementAt(Me.Rotors(2).Offset))

			' Check if middle rotor is at notch position
			middleNotched = Me.Rotors(1).Notches.Contains(Me.Rotors(1).Mappings.Keys.ElementAt(Me.Rotors(1).Offset))
		End If

		' Step according to Enigma rules:

		' Step 1: If middle rotor is at notch, both middle and left rotors step
		If middleNotched And UBound(Me.Rotors) >= 2 Then
			Me.Rotors(0).Rotate() ' Left rotor steps
			Me.Rotors(1).Rotate() ' Middle rotor steps
		End If

		' Step 2: If right rotor is at notch, middle rotor steps
		' (Note: middle rotor can step twice in one operation - the double stepping)
		If rightNotched And UBound(Me.Rotors) >= 2 Then
			Me.Rotors(1).Rotate() ' Middle rotor steps
		End If

		' Step 3: Right rotor always steps
		Me.Rotors(UBound(Me.Rotors)).Rotate() ' Right-most rotor always rotates
	End Sub


	Public Function EncryptChar(ByVal PlainChar As Char) As Char
		'' Encrypts a character by encrypting it with the first rotor and then
		'' passing the result into the next rotor.
		'' Calls EnigmaMachine.RotateRotors() before fully encrypting a character

		Dim CipherChar As Char = PlainChar

		' First rotate the rotors so the next character uses a new alphabet.
		Me.RotateRotors()

		' Pass the character through the rotors from right to left.
		For Each Rot As Rotor In Me.Rotors.Reverse()
			CipherChar = Rot.StandardEncrypt(CipherChar)
		Next

		' Reflect the character once through all the rotors.
		If Char.IsLetter(CipherChar) Then
			If Reflector.ContainsKey(CipherChar) Then
				CipherChar = Reflector(CipherChar)
			Else
				Throw New TranslationException("Reflector could not map " & CipherChar)
			End If
		End If

		' Pass the character through the rotors from left to right.
		For Each Rot As Rotor In Me.Rotors
			CipherChar = Rot.ReflectEncrypt(CipherChar)
		Next

		Return CipherChar
	End Function


	Public Function EncryptText(ByVal PlainText As String, Optional ByVal formatInGroups As Boolean = True) As String
		If String.IsNullOrEmpty(PlainText) Then
			Return String.Empty
		End If

		' Convert to uppercase and remove all non-alphabetic characters
		Dim filteredText As String = ""
		For Each c As Char In PlainText.ToUpper()
			If c >= "A"c AndAlso c <= "Z"c Then
				filteredText += c
			End If
		Next

		' Encrypt the filtered text
		Dim CipherText As String = ""
		For Each PlainChar As Char In filteredText
			CipherText += Me.EncryptChar(PlainChar)
		Next

		' Format in groups of 5 letters if requested
		If formatInGroups Then
			Dim formattedText As String = ""
			For i As Integer = 0 To CipherText.Length - 1
				formattedText += CipherText(i)
				If (i + 1) Mod 5 = 0 AndAlso i < CipherText.Length - 1 Then
					formattedText += " "
				End If
			Next
			Return formattedText
		Else
			Return CipherText
		End If
	End Function




End Class
