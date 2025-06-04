Imports ConsoleEnigmaMachine.My.Resources

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

		While CBool(1)
			' Display prompt (only for subsequent entries after first prompt) 
			'firstInput = False

			Dim plainText As String = Console.ReadLine().ToUpper()

			If String.IsNullOrEmpty(plainText) Then
				Console.WriteLine(String.Format(Strings.strInputNull))
				Continue While
			End If

			If plainText = "--Q" Then
				Console.WriteLine(String.Format(Strings.strQuit))
				Exit While

			ElseIf plainText.StartsWith("--E") Then
				HandleEncryptCommand(machine, plainText)

				If easyMode Then
					rotors = MachineBuilder.RotorsFromXMLData(rotorsXML).Take(3).ToArray
					reflector = MachineBuilder.ReflectorFromXMLData(reflectorsXML)
					machine = New EnigmaMachine(rotors, reflector)
					' Auto-fill rotor positions from initialRotorPositions
					If initialRotorPositions IsNot Nothing Then
						For i = 0 To Math.Min(2, rotors.Length - 1)
							machine.Rotors(i).offset = initialRotorPositions(i)
						Next
						Console.WriteLine("Easy mode: Rotors auto-reset to initial positions: " &
				String.Join(", ", initialRotorPositions.Select(Function(pos, idx) $"rotor {idx + 1} @ {pos}")))
						PrintModeBanner()
					Else
						Console.WriteLine(String.Format(Strings.strSavedPosErr))
						PreRotateRotors(machine) ' fallback if not set
					End If
				End If
				Continue While

			ElseIf plainText.StartsWith("--D") Then
				HandleDecryptCommand(machine, plainText)
				Continue While

			ElseIf plainText.StartsWith("--MODE") Then
				HandleModeCommand(plainText)
				Continue While

			ElseIf plainText = "--R" Then
				Console.WriteLine(String.Format(Strings.strReset))
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
			Catch ex As translationException
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
		Console.WriteLine(Strings.strPreRotRotors)

		Dim rotorPositions(UBound(machine.Rotors)) As Integer
		Dim random As New Random()

		For i As Integer = 0 To Math.Min(2, UBound(machine.Rotors))
			Dim validInput As Boolean = False
			Dim rotorNumber As Integer = i + 1
			Dim preRotations As Integer

			While Not validInput
				Console.WriteLine(String.Format(Strings.strPromRotorPos, rotorNumber))
				Dim input As String = Console.ReadLine()

				If IsNumeric(input) Then
					preRotations = CInt(Val(input))
					If preRotations >= 0 AndAlso preRotations <= 25 Then
						validInput = True
						rotorPositions(i) = preRotations

						' Simulate occasional mechanical issues (3% chance)
						If random.Next(100) < 3 Then
							Console.WriteLine(String.Format(Strings.strWarnResistRotors, rotorNumber))
							System.Threading.Thread.Sleep(1500) ' Simulate adjustment time

							If random.Next(100) < 30 Then ' 30% of mechanical issues cause a problem
								' Simulate slippage - actual position differs from intended
								Dim slippage As Integer = If(random.Next(2) = 0, 1, -1) ' Slip by +1 or -1
								rotorPositions(i) = (rotorPositions(i) + slippage + 26) Mod 26
								Console.WriteLine(String.Format(Strings.strNoteSettleRotor, rotorNumber, rotorPositions(i)))
							Else
								Console.WriteLine(String.Format(Strings.strNoteSettleOK, rotorNumber))
							End If
						End If

					Else
						Console.WriteLine(String.Format(Strings.strErrRotorValues))
					End If
				Else
					Console.WriteLine(String.Format(Strings.strErrNumeric))
				End If
			End While

			' Perform the actual rotation
			For j As Integer = 1 To rotorPositions(i) Mod 26
				machine.Rotors(i).Rotate()
			Next

			' Simulate contact problems (2% chance)
			If random.Next(100) < 2 Then
				Console.WriteLine(String.Format(Strings.strWarnEC, rotorNumber))

				'set the flag here to occasionally introduce errors during encryption
				''''machine.Rotors(i).hasContactIssues = True
			End If
		Next

		' Display confirmation of the rotor positions
		Console.WriteLine(String.Format(Strings.strSuccessRot))

		For i As Integer = 0 To Math.Min(2, UBound(machine.Rotors))
			Console.Write("rotor " & (i + 1) & " @ " & rotorPositions(i))
			If i < UBound(machine.Rotors) Then
				Console.Write(", ")
			ElseIf i = UBound(machine.Rotors) - 1 AndAlso UBound(machine.Rotors) > 1 Then
				Console.Write(" and ")
			End If
		Next

		'store initial rotor positions for easy mode reset
		initialRotorPositions = CType(rotorPositions.Clone(), Integer())

		Console.WriteLine()
		PrintModeBanner()
	End Sub

	''' <summary>
	''' Outputs the current Enigma machine settings to the console.
	''' </summary>
	''' <param name="machine">The EnigmaMachine instance.</param>
	Sub OutputEnigmaSettings(machine As EnigmaMachine)
		Console.WriteLine(String.Format(Strings.strMachineInit))

		' Output rotor IDs (e.g., "Rotor 1", "Rotor 2", ...)
		Console.WriteLine("Rotors: " & String.Join(",", machine.Rotors.Select(Function(r) r.iD)))

		' Output rotor offsets as numeric positions (0-25)
		Console.WriteLine("Positions: " & String.Join(",", machine.Rotors.Select(Function(r) r.offset.ToString())))

		Console.WriteLine(String.Format(Strings.strSettingsNA))
	End Sub

	''' <summary>
	''' Prints the current mode banner and available commands.
	''' </summary>
	Sub PrintModeBanner()
		Console.WriteLine(String.Format(Strings.strLine))

		If easyMode Then
			Console.WriteLine(String.Format(Strings.strEasyMode))
		Else
			Console.WriteLine(String.Format(Strings.strNotEasyMode))
		End If

		Console.WriteLine(String.Format(Strings.strLine))
	End Sub

	''' <summary>
	''' Handles the --MODE command to switch between EASY and REAL modes.
	''' </summary>
	''' <param name="input">The user input string.</param>
	Sub HandleModeCommand(input As String)
		Dim mode = input.Substring(7).Trim().ToUpper()
		If mode = "EASY" Then
			easyMode = True
			Console.WriteLine(String.Format(Strings.strModeCmdEZ))
		ElseIf mode = "REAL" Then
			easyMode = False
			Console.WriteLine(String.Format(Strings.strModeCmdReal))
		Else
			Console.WriteLine(String.Format(Strings.strModeCmdErr))
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
			Console.WriteLine(String.Format(Strings.strDecryptSyntax))
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
			Console.WriteLine(String.Format(Strings.strEncryptSyntax))
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
