Module Module1

	'' Manual testing interface.

	Dim RotorsXML As String = My.Resources.German123
	Dim ReflectorsXML As String = My.Resources.ReflectorB
	Dim formatInGroups As Boolean = True  ' Default to historical formatting


	Sub Main()
		Dim Rotors() As Rotor
		Dim Reflector As Dictionary(Of Char, Char)
		Dim Machine As EnigmaMachine
		Dim firstInput As Boolean = True

		Rotors = MachineBuilder.RotorsFromXMLData(RotorsXML)
		Reflector = MachineBuilder.ReflectorFromXMLData(ReflectorsXML)

		Machine = New EnigmaMachine(Rotors, Reflector)
		PreRotateRotors(Machine)

		While 1
			' Display prompt (only for subsequent entries after first prompt)
			If Not firstInput Then
				Console.WriteLine("---------------------------------------")
				Console.WriteLine("Type your text here (or --Q to quit, --R to reset, --F to toggle 5-letter formatting):")
			End If
			firstInput = False

			Dim PlainText As String = Console.ReadLine().ToUpper()

			If String.IsNullOrEmpty(PlainText) Then
				Console.WriteLine("Please enter some text to encrypt.")
				Continue While
			End If

			If PlainText = "--Q" Then
				Console.WriteLine("Quitting...")
				Exit While
			ElseIf PlainText = "--R" Then
				Console.WriteLine("Resetting...")
				Rotors = MachineBuilder.RotorsFromXMLData(RotorsXML)
				Reflector = MachineBuilder.ReflectorFromXMLData(ReflectorsXML)
				Machine = New EnigmaMachine(Rotors, Reflector)
				PreRotateRotors(Machine)
				Continue While
			ElseIf PlainText = "--F" Then
				formatInGroups = Not formatInGroups
				Console.WriteLine("5-letter formatting is now: " & If(formatInGroups, "ON (historical)", "OFF"))
				Continue While
			End If

			Try
				Console.WriteLine(Machine.EncryptText(PlainText, formatInGroups))
			Catch ex As TranslationException
				Console.WriteLine("Error: " & ex.Message)
			Catch ex As Exception
				Console.WriteLine("An unexpected error occurred: " & ex.Message)
			End Try
		End While

	End Sub

	Public Sub PreRotateRotors(ByRef Machine As EnigmaMachine)
		' Display information about rotor rotation limits
		Console.WriteLine("=== ENIGMA MACHINE CONFIGURATION ===")
		Console.WriteLine("Enter rotation settings for each rotor (0-25).")
		Console.WriteLine("These values represent the initial position of each rotor.")
		Console.WriteLine("---------------------------------------")

		Dim rotorPositions(UBound(Machine.Rotors)) As Integer
		Dim random As New Random()

		For i As Integer = 0 To UBound(Machine.Rotors)
			Dim validInput As Boolean = False
			Dim rotorNumber As Integer = i + 1
			Dim preRotations As Integer = 0

			While Not validInput
				Console.Write("Pre-rotate rotor " & rotorNumber & " (0-25): ")
				Dim input As String = Console.ReadLine()

				If IsNumeric(input) Then
					preRotations = Val(input)
					If preRotations >= 0 AndAlso preRotations <= 25 Then
						validInput = True
						rotorPositions(i) = preRotations

						' Simulate occasional mechanical issues (3% chance)
						If random.Next(100) < 3 Then
							Console.WriteLine("Warning: Slight resistance detected when setting rotor " & rotorNumber & ".")
							Console.WriteLine("Attempting to adjust...")
							System.Threading.Thread.Sleep(1500) ' Simulate adjustment time

							If random.Next(100) < 30 Then ' 30% of mechanical issues cause a problem
								' Simulate slippage - actual position differs from intended
								Dim slippage As Integer = If(random.Next(2) = 0, 1, -1) ' Slip by +1 or -1
								rotorPositions(i) = (rotorPositions(i) + slippage + 26) Mod 26
								Console.WriteLine("Note: Rotor " & rotorNumber & " settled at position " & rotorPositions(i) & " (slight misalignment).")
							Else
								Console.WriteLine("Rotor " & rotorNumber & " successfully adjusted.")
							End If
						End If

					Else
						Console.WriteLine("Error: Please enter a value between 0 and 25.")
					End If
				Else
					Console.WriteLine("Error: Please enter a numeric value.")
				End If
			End While

			' Perform the actual rotation
			For j As Integer = 1 To rotorPositions(i) Mod Machine.Rotors(i).Mappings.Count
				Machine.Rotors(i).Rotate()
			Next

			' Simulate contact problems (2% chance)
			If random.Next(100) < 2 Then
				Console.WriteLine("Warning: Poor electrical contact detected in rotor " & rotorNumber & ".")
				Console.WriteLine("This may cause occasional encryption errors.")
				' You could set a flag here to occasionally introduce errors during encryption
			End If
		Next

		' Display confirmation of the rotor positions
		Console.WriteLine("---------------------------------------")
		Console.Write("Rotors have been successfully pre-rotated as follows: ")

		For i As Integer = 0 To UBound(Machine.Rotors)
			Console.Write("rotor " & (i + 1) & " @ " & rotorPositions(i))
			If i < UBound(Machine.Rotors) Then
				Console.Write(", ")
			ElseIf i = UBound(Machine.Rotors) - 1 AndAlso UBound(Machine.Rotors) > 1 Then
				Console.Write(" and ")
			End If
		Next

		Console.WriteLine()
		Console.WriteLine("---------------------------------------")
		Console.WriteLine("Machine has been successfully configured--you may begin entering text to encrypt.")
		Console.WriteLine("Type your text here: (or type --Q to quit, --R to reset, --F to toggle 5-letter formatting):")
	End Sub

End Module
