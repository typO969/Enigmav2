''' <summary>
''' Represents a single Enigma rotor with fixed wiring, notches, and position offset.
''' </summary>
Public Class Rotor
	Public ReadOnly wiring As String ' e.g. "EKMFLGDQVZNTOWYHXUSPAIBRCJ"
	Public ReadOnly notches As Char()
	Public Property offset As Integer = 0
	Public Property iD As String
	Public Property ringSetting As Integer ' 0-25, where 0 = 'A'
	Public hasContactIssues As Boolean = False

	''' <summary>
	''' Initializes a new instance of the Rotor class with specified wiring, notches, and optional ID.
	''' </summary>
	''' <param name="wiring">A 26-character string representing the rotor wiring (A-Z order).</param>
	''' <param name="notches">Characters indicating notch positions for turnover.</param>
	''' <param name="id">Optional rotor identifier.</param>
	Public Sub New(wiring As String, notches() As Char, Optional iD As String = "", Optional ringSetting As Integer = 0)
		Me.wiring = wiring
		Me.notches = notches
		Me.iD = iD
		offset = 0
		Me.ringSetting = ringSetting
	End Sub

	' Adjust your stepping and mapping logic to account for ringSetting
	' For example, when mapping input to wiring, use:
	' (input + offset - ringSetting + 26) Mod 26



	''' <summary>
	''' Encrypts a character passing through the rotor from right to left, applying the current offset.
	''' </summary>
	''' <param name="char1">The character to encrypt.</param>
	''' <returns>The encrypted character.</returns>
	Public Function StandardEncrypt(ByVal char1 As Char) As Char
		If hasContactIssues AndAlso (New Random()).Next(100) < 5 Then
			' Simulate contact issue: return a random letter
			Return ChrW(Asc("A") + (New Random()).Next(26))
		End If

		If Not Char.IsLetter(char1) Then Return char1

		Dim pos As Integer = (Asc(Char.ToUpper(char1)) - Asc("A") + offset) Mod 26
		Return wiring(pos)
	End Function

	''' <summary>
	''' Encrypts a character passing through the rotor from left to right (reverse path), applying the current offset.
	''' </summary>
	''' <param name="char1">The character to encrypt.</param>
	''' <returns>The encrypted character.</returns>
	Public Function ReflectEncrypt(ByVal char1 As Char) As Char
		If Not Char.IsLetter(char1) Then Return char1

		Dim idx As Integer = wiring.IndexOf(Char.ToUpper(char1))
		If idx = -1 Then Throw New translationException("Could not map " & char1)
		Dim pos As Integer = (idx - offset + 26) Mod 26
		Return ChrW(Asc("A") + pos)
	End Function


	''' <summary>
	''' Determines whether the specified character can be encrypted by the rotor.
	''' </summary>
	''' <param name="chr">The character to check.</param>
	''' <returns>True if the character is a letter; otherwise, false.</returns>
	Public Shared Function CanEncrypt(ByVal chr As Char) As Boolean
		Return Char.IsLetter(chr)
	End Function

	''' <summary>
	''' Advances the rotor's offset by one position, simulating a physical rotation.
	''' </summary>
	Public Sub Rotate()
		offset = (offset + 1) Mod 26
	End Sub

End Class

''' <summary>
''' Exception thrown when a character cannot be mapped during translation.
''' </summary>
Class translationException
	Inherits Exception

	Public Sub New()
	End Sub

	Public Sub New(ByVal message As String)
		MyBase.New(message)
	End Sub

	Public Sub New(ByVal message As String, ByVal innerException As Exception)
		MyBase.New(message, innerException)
	End Sub

End Class