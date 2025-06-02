''' <summary>
''' Provides the main console interface for manual Enigma machine testing and interaction.
''' </summary>
Public Module Module1
	' Manual testing interface.
	Dim rotorsXML As String = My.Resources.German123
	Dim reflectorsXML As String = My.Resources.ReflectorB
	Dim formatInGroups As Boolean = True  ' Default to historical formatting
	Public easyMode As Boolean = True
	Dim initialRotorPositions() As Integer


	''' <summary>
	''' Main entry point for the Enigma console application.
	''' </summary>
	Sub Main()
		Dim rotors() As Rotor
		Dim reflector As String
		Dim machine As EnigmaMachine
		Dim firstInput As Boolean = True

		rotors = MachineBuilder.RotorsFromXMLData(rotorsXML).Take(3).ToArray()
		reflector = MachineBuilder.ReflectorFromXMLData(reflectorsXML)

		machine = New EnigmaMachine(rotors, reflector)
		PreRotateRotors(machine)

		While 1
			' Display prompt (only for subsequent entries after first prompt) 
			'firstInput = False

			Dim plainText As String = Console.ReadLine().ToUpper()

			If String.IsNullOrEmpty(plainText) Then
				Console.WriteLine("Please enter some text to encrypt.")
				Continue While
			End If

			If plainText = "--Q" Then
				Console.WriteLine("Quitting...")
				Exit While

			ElseIf plainText.StartsWith("--E") Then
				HandleEncryptCommand(machine, plainText)

				If easyMode Then
					rotors = MachineBuilder.RotorsFromXMLData(rotorsXML).Take(3).ToArray
					reflector = MachineBuilder.ReflectorFromXMLData(reflectorsXML)
					machine = New EnigmaMachine(rotors, reflector)
					PreRotateRotors(machine)
				End If

				Continue While

			ElseIf plainText.StartsWith("--D") Then
				HandleDecryptCommand(machine, plainText)
				Continue While

			ElseIf plainText.StartsWith("--MODE") Then
				HandleModeCommand(plainText)
				Continue While

			ElseIf plainText = "--R" Then
				Console.WriteLine("Resetting...")
				rotors = MachineBuilder.RotorsFromXMLData(rotorsXML).Take(3).ToArray
				reflector = MachineBuilder.ReflectorFromXMLData(reflectorsXML)
				machine = New EnigmaMachine(rotors, reflector)
				PreRotateRotors(machine)
				Continue While

			ElseIf plainText = "--F" Then
				formatInGroups = Not formatInGroups
				Console.WriteLine("5-letter formatting is now: " & If(formatInGroups, "ON (historical)", "OFF"))
				Continue While

			End If

			Try
				Console.WriteLine(machine.EncryptText(plainText, formatInGroups))
				OutputEnigmaSettings(machine)
			Catch ex As TranslationException
				Console.WriteLine("Error: " & ex.Message)
			Catch ex As Exception
				Console.WriteLine("An unexpected error occurred: " & ex.Message)
			End Try
		End While

	End Sub

	''' <summary>
	''' Prompts the user to set initial rotor positions and simulates mechanical issues.
	''' </summary>
	''' <param name="machine">The EnigmaMachine instance to configure.</param>
	Public Sub PreRotateRotors(ByRef machine As EnigmaMachine)
		' Display information about rotor rotation limits
		Console.WriteLine("=== ENIGMA MACHINE CONFIGURATION ===")
		Console.WriteLine("Enter rotation settings for each rotor (0-25).")
		Console.WriteLine("These values represent the initial position of each rotor.")
		Console.WriteLine("---------------------------------------")

		Dim rotorPositions(UBound(machine.Rotors)) As Integer
		Dim random As New Random()

		For i As Integer = 0 To Math.Min(2, UBound(machine.Rotors))
			Dim validInput As Boolean = False
			Dim rotorNumber As Integer = i + 1
			Dim preRotations As Integer

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
			For j As Integer = 1 To rotorPositions(i) Mod 26
				machine.Rotors(i).Rotate()
			Next

			' Simulate contact problems (2% chance)
			If random.Next(100) < 2 Then
				Console.WriteLine("Warning: Poor electrical contact detected in rotor " & rotorNumber & ".")
				Console.WriteLine("This may cause occasional encryption errors.")
				'set the flag here to occasionally introduce errors during encryption
				''''machine.Rotors(i).hasContactIssues = True
			End If
		Next

		' Display confirmation of the rotor positions
		Console.WriteLine("---------------------------------------")
		Console.Write("Rotors have been successfully pre-rotated as follows: ")

		For i As Integer = 0 To Math.Min(2, UBound(machine.Rotors))
			Console.Write("rotor " & (i + 1) & " @ " & rotorPositions(i))
			If i < UBound(machine.Rotors) Then
				Console.Write(", ")
			ElseIf i = UBound(machine.Rotors) - 1 AndAlso UBound(machine.Rotors) > 1 Then
				Console.Write(" and ")
			End If
		Next

		initialRotorPositions = rotorPositions.Clone()
		Console.WriteLine()
		PrintModeBanner()
	End Sub

	''' <summary>
	''' Outputs the current Enigma machine settings to the console.
	''' </summary>
	''' <param name="machine">The EnigmaMachine instance.</param>
	Sub OutputEnigmaSettings(machine As EnigmaMachine)
		Console.WriteLine("--- Enigma Machine Settings ---")

		' Output rotor IDs (e.g., "Rotor 1", "Rotor 2", ...)
		Console.WriteLine("Rotors: " & String.Join(",", machine.Rotors.Select(Function(r) r.iD)))

		' Output rotor offsets as numeric positions (0-25)
		Console.WriteLine("Positions: " & String.Join(",", machine.Rotors.Select(Function(r) r.offset.ToString())))

		Console.WriteLine("Ring Settings: (not exposed)")
		Console.WriteLine("Plugboard: (not available)")
		Console.WriteLine("--------------------------------")
	End Sub

	''' <summary>
	''' Prints the current mode banner and available commands.
	''' </summary>
	Sub PrintModeBanner()
		Console.WriteLine("-------------------------------------")

		If easyMode Then
			Console.WriteLine()
			Console.WriteLine()
			Console.WriteLine("GERMAN ENIGMA MACHINE v2 COMMAND LINE")
			Console.WriteLine("-------------------------------------")
			Console.WriteLine("You are currently in EASY mode of operation. The machine will auto reset after encrypting text.")
			Console.WriteLine(" To switch to HISTORICALLY ACCURATE mode type: '--MODE REAL'")
			Console.WriteLine("═══════════════════════")
			Console.WriteLine("The Enigma Commands: --E, --D, --F, --R, --MODE, --Q")
			Console.WriteLine("To ENCRYPT a message: '--E <msg to encrypt>'.   To DECRYPT a cipher text: '--D <encrypted msg>,rotor 1 settings,r2,r3'")
			Console.WriteLine("To DISABLE grouping the cipher text in sets of 5: '--F'.  To RESET the Enigma machine and its rotors: '--R'.  To QUIT this program: '--Q'")
		Else
			Console.WriteLine()
			Console.WriteLine()
			Console.WriteLine("DEUTSCHE ENIGMA-MASCHINE v2 EINGABEAUFFORDERUNG")
			Console.WriteLine("-------------------------------------")
			Console.WriteLine("You are currently in REAL mode of operation. The machine will not auto reset after encrypting text.")
			Console.WriteLine(" To switch to EASY mode type: '--MODE EASY'")
			Console.WriteLine("═══════════════════════")
			Console.WriteLine("Die Enigma-Befehle: --E, --D, --F, --R, --MODE, --Q")
			Console.WriteLine("So VERSCHLÜSSELN Sie eine Nachricht: '--E <zu verschlüsselnde Nachricht>'. So ENTSCHLÜSSELN Sie einen Geheimtext: '--D <verschlüsselte Nachricht>,Rotor 1-Einstellungen,R2,R3'.")
			Console.WriteLine("Um die Gruppierung des Geheimtextes in Fünfgruppen zu DEAKTIVIEREN: '--F'. Um die Enigma-Maschine und ihre Rotoren ZURÜCKZUSETZEN: '--R'. Um das Programm zu BEENDEN: '--Q'")
		End If

		Console.WriteLine("-------------------------------------")
	End Sub

	''' <summary>
	''' Handles the --MODE command to switch between EASY and REAL modes.
	''' </summary>
	''' <param name="input">The user input string.</param>
	Sub HandleModeCommand(input As String)
		Dim mode = input.Substring(7).Trim().ToUpper()
		If mode = "EASY" Then
			easyMode = True
			Console.WriteLine("Mode set to EASY (auto-reset and simplified use).")
		ElseIf mode = "REAL" Then
			easyMode = False
			Console.WriteLine("Mode set to REALISTIC (manual resets and full control).")
		Else
			Console.WriteLine("Unknown mode. Use: --MODE EASY or --MODE REAL")
			Return
		End If
		PrintModeBanner()
	End Sub

	''' <summary>
	''' Handles the --D (decrypt) command, including rotor position setup.
	''' </summary>
	''' <param name="machine">The EnigmaMachine instance (ByRef).</param>
	''' <param name="input">The user input string.</param>
	Sub HandleDecryptCommand(ByRef machine As EnigmaMachine, input As String)

		Dim payload = input.Substring(4).Trim() ' strip "--D "
		Dim parts = payload.Split(New Char() {","c}, StringSplitOptions.None).Select(Function(s) s.Trim()).ToArray()

		If parts.Length < machine.Rotors.Length + 1 Then
			Console.WriteLine("Usage: --D <ciphertext>,<pos1>,<pos2>,<pos3>")
			Return
		End If
		' Clean input: remove all non-letters
		Dim cleaned = New String(parts(0).Where(Function(c) Char.IsLetter(c)).ToArray())
		Dim cipherText = cleaned.ToUpper()
		Dim output = machine.EncryptText(cipherText)
		Dim outputNoSpaces = output.Replace(" ", "")

		For i = 0 To machine.Rotors.Length - 1
			Dim val As Integer
			If Not Integer.TryParse(parts(i + 1), val) OrElse val < 0 OrElse val > 25 Then
				Console.WriteLine("Invalid rotor position: " & parts(i + 1))
				Return
			End If
			machine.Rotors(i).offset = val
		Next

		If easyMode Then
			Dim rotors = MachineBuilder.RotorsFromXMLData(rotorsXML).Take(3).ToArray()
			Dim reflector = MachineBuilder.ReflectorFromXMLData(reflectorsXML)
			machine = New EnigmaMachine(rotors, reflector)
			' Set rotors to initial positions
			If initialRotorPositions IsNot Nothing Then
				For i = 0 To Math.Min(2, rotors.Length - 1)
					machine.Rotors(i).offset = initialRotorPositions(i)
				Next
			End If
			outputNoSpaces = DecodeInitialSpaces(outputNoSpaces)
		End If


		Console.WriteLine("Decrypted: " & outputNoSpaces)

	End Sub

	''' <summary>
	''' Handles the --E (encrypt) command.
	''' </summary>
	''' <param name="machine">The EnigmaMachine instance.</param>
	''' <param name="input">The user input string.</param>
	Sub HandleEncryptCommand(machine As EnigmaMachine, input As String)
		Dim message = input.Substring(4).TrimEnd()
		If easyMode Then
			message = EncodeInitialSpaces(message)
		End If
		message = message.ToUpper()
		If String.IsNullOrWhiteSpace(message) Then
			Console.WriteLine("Usage: --E <plaintext message>")
			Return
		End If

		Dim output = machine.EncryptText(message)
		Console.WriteLine("Encrypted: " & output)
		OutputEnigmaSettings(machine)
	End Sub


	' Replace leading spaces with U+200B
	Function EncodeInitialSpaces(input As String) As String
		Dim sb As New System.Text.StringBuilder()
		Dim i As Integer = 0
		While i < input.Length AndAlso input(i) = " "c
			sb.Append(ChrW(&H200B))
			i += 1
		End While
		If i < input.Length Then sb.Append(input.Substring(i))
		Return sb.ToString()
	End Function


	' Replace leading U+200B with spaces
	Function DecodeInitialSpaces(input As String) As String
		Dim sb As New System.Text.StringBuilder()
		Dim i As Integer = 0
		While i < input.Length AndAlso input(i) = ChrW(&H200B)
			sb.Append(" "c)
			i += 1
		End While
		If i < input.Length Then sb.Append(input.Substring(i))
		Return sb.ToString()
	End Function


End Module
