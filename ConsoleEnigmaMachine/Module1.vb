Imports ConsoleEnigmaMachine.My.Resources

''' <summary>
''' Provides the main console interface for manual Enigma machine testing and interaction.
''' </summary>
Public Module Module1
	' Manual testing interface.
	Dim formatInGroups As Boolean = True  ' Default to historical formatting
	Public easyMode As Boolean = True
	Dim initialRotorPositions() As Integer
	Dim selectedRotors As List(Of Rotor)
	Dim reflectorWiring As String
	' Instead of loading from file:
	' Dim rotorPath As String = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "My Project\rotors.xml")
	' Dim rotors As Rotor() = RotorsFromXMLFilePath(rotorPath)

	' Load from resources:
	Dim rotors As Rotor() = RotorsFromXMLData(My.Resources.rotors)

	''' <summary>
	''' Main entry point for the Enigma console application.
	''' </summary>
	Sub Main()
		Dim machine As EnigmaMachine

		' Load all available rotors from XML
		Dim allRotors = rotors
		' Filter to only I-V
		Dim allowedIDs = New String() {"I", "II", "III", "IV", "V"}
		Dim availableRotors = allRotors.Where(Function(r) allowedIDs.Contains(r.iD)).ToList()

		' Prompt user for rotor selection and order
		selectedRotors = New List(Of Rotor)
		Dim usedIDs As New List(Of String)
		Dim positions = New String() {"left (slow)", "middle (medium)", "right (fast)"}

		'master rotor loader
		For i = 0 To 2
			Dim rotorID As String = ""
			Do
				Console.WriteLine($"Select rotor for {positions(i)} position (I, II, III, IV, V):")
				rotorID = Console.ReadLine().Trim().ToUpper()
			Loop Until allowedIDs.Contains(rotorID) AndAlso Not usedIDs.Contains(rotorID)
			selectedRotors.Add(availableRotors.First(Function(r) r.iD = rotorID))
			usedIDs.Add(rotorID)

			' Prompt for ring setting immediately after rotor selection
			Console.WriteLine($"Enter ring setting (1-26, where 1=A) for {rotorID}:")
			Dim ringInput As String = Console.ReadLine().Trim()
			Dim ringSetting As Integer = 1
			If Integer.TryParse(ringInput, ringSetting) AndAlso ringSetting >= 1 AndAlso ringSetting <= 26 Then
				selectedRotors(i).ringSetting = ringSetting - 1 ' 0-based
			Else
				selectedRotors(i).ringSetting = 0
			End If
		Next

		' Load reflector B
		' Instead of:
		' reflectorWiring = MachineBuilder.ReflectorFromXMLFilePath("My Project\reflectors.xml")
		' Use:
		reflectorWiring = MachineBuilder.ReflectorFromXMLData(My.Resources.reflectors)


		' Show summary
		Console.WriteLine("Selected rotors (left to right): " & String.Join(", ", selectedRotors.Select(Function(r) r.iD)))
		Console.WriteLine("Reflector: B")

		' Now you can pass selectedRotors and reflectorWiring to your EnigmaMachine constructor
		' Example:
		' Dim machine As New EnigmaMachine(selectedRotors.ToArray(), reflectorWiring)

		machine = New EnigmaMachine(selectedRotors.ToArray(), reflectorWiring)
		PreRotateRotors(machine)

		While True
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
					machine = New EnigmaMachine(selectedRotors.ToArray(), reflectorWiring)
					If initialRotorPositions IsNot Nothing Then
						For i = 0 To Math.Min(2, selectedRotors.Count - 1)
							machine.Rotors(i).offset = initialRotorPositions(i)
						Next
						Console.WriteLine("Easy mode: Rotors auto-reset to initial positions: " &
				String.Join(", ", initialRotorPositions.Select(Function(pos, idx) $"rotor {idx + 1} @ {pos}")))
						PrintModeBanner()
					Else
						Console.WriteLine(String.Format(Strings.strSavedPosErr))
						PreRotateRotors(machine)
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
				machine = New EnigmaMachine(selectedRotors.ToArray(), reflectorWiring)
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
		initialRotorPositions = DirectCast(rotorPositions.Clone(), Integer())


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
		For i = 0 To machine.Rotors.Length - 1
			Dim val As Integer
			If Not Integer.TryParse(parts(i + 1), val) OrElse val < 0 OrElse val > 25 Then
				Console.WriteLine("Invalid rotor position: " & parts(i + 1))
				Return
			End If
			machine.Rotors(i).offset = val
		Next

		' Clean input: remove all non-letters
		Dim cleaned = New String(parts(0).Where(Function(c) Char.IsLetter(c)).ToArray())
		Dim cipherText = cleaned.ToUpper()
		Dim output = machine.EncryptText(cipherText)
		Dim outputNoSpaces = output.Replace(" ", "")

		If easyMode Then
			machine = New EnigmaMachine(selectedRotors.ToArray(), reflectorWiring)
			If initialRotorPositions IsNot Nothing Then
				For i = 0 To Math.Min(2, selectedRotors.Count - 1)
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
