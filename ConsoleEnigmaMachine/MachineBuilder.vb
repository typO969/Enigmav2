Imports System.Xml

''' <summary>
''' Provides methods to construct Enigma rotors and reflector from XML data.
''' </summary>
Module MachineBuilder

	''' <summary>
	''' Loads reflector wiring from an XML file path.
	''' </summary>
	''' <param name="Path">The file path to the reflector XML.</param>
	''' <returns>Reflector wiring as a 26-character string.</returns>
	Public Function ReflectorFromXMLFilePath(ByVal Path As String) As String
		' Given a file path, returns a dictionary containing the reflector character mappings.
		Dim XMLDoc As New XmlDocument
		XMLDoc.Load(Path)

		Return ReflectorFromXMLDocument(XMLDoc)
	End Function

	''' <summary>
	''' Loads reflector wiring from raw XML data.
	''' </summary>
	''' <param name="XMLData">The reflector XML as a string.</param>
	''' <returns>Reflector wiring as a 26-character string.</returns>
	Public Function ReflectorFromXMLData(ByVal XMLData As String) As String
		' Given raw XML data, returns a dictionary containing the reflector character mappings.
		Dim XMLDoc As New XmlDocument
		XMLDoc.LoadXml(XMLData)

		Return ReflectorFromXMLDocument(XMLDoc)
	End Function

	''' <summary>
	''' Loads reflector wiring from an XmlDocument.
	''' </summary>
	''' <param name="XMLDoc">The reflector XML document.</param>
	''' <returns>Reflector wiring as a 26-character string.</returns>
	Public Function ReflectorFromXMLDocument(ByRef XMLDoc As XmlDocument) As String
		' Given a .NET XMLDocument object, returns a dictionary containing the reflector character mappings.
		Dim ReflectorNode As XmlNode = XMLDoc.Item("reflector")

		Return WiringFromXML(ReflectorNode)
	End Function

	''' <summary>
	''' Loads an array of rotors from an XML file path.
	''' </summary>
	''' <param name="Path">The file path to the rotors XML.</param>
	''' <returns>Array of Rotor objects.</returns>
	Public Function RotorsFromXMLFilePath(ByVal Path As String) As Rotor()
		' Given a file path to an XML file, returns an array of rotors.
		Dim XMLDoc As New XmlDocument
		XMLDoc.Load(Path)

		Return RotorsFromXMLDocument(XMLDoc)
	End Function

	''' <summary>
	''' Loads an array of rotors from raw XML data.
	''' </summary>
	''' <param name="XMLData">The rotors XML as a string.</param>
	''' <returns>Array of Rotor objects.</returns>
	Public Function RotorsFromXMLData(ByVal XMLData As String) As Rotor()
		' Given raw XML data, returns an array of rotors.
		Dim XMLDoc As New XmlDocument
		XMLDoc.LoadXml(XMLData)

		Return RotorsFromXMLDocument(XMLDoc)
	End Function

	''' <summary>
	''' Loads an array of rotors from an XmlDocument.
	''' </summary>
	''' <param name="XMLDoc">The rotors XML document.</param>
	''' <returns>Array of Rotor objects.</returns>
	Public Function RotorsFromXMLDocument(ByRef XMLDoc As XmlDocument) As Rotor()
		''Given a .NET XMLDocument, constructs an array of rotors.
		Dim RotorNodes As XmlNodeList = XMLDoc.GetElementsByTagName("rotor")
		Dim Rotors As New List(Of Rotor)

		' Find rotors which don't have a specific position set and insert them in order of appearance.
		For Each RotorNode As XmlNode In RotorNodes
			Dim notches() As Char = RotorNode.Attributes.ItemOf("notches").Value.ToCharArray()
			Dim wiring As String = WiringFromXML(RotorNode)
			Dim positionAttr = RotorNode.Attributes.ItemOf("position")
			Dim rotorID As String = If(positionAttr IsNot Nothing, "Rotor " & positionAttr.Value, "Unknown")
			Rotors.Add(New Rotor(wiring, notches, rotorID))
		Next

		' Find rotors which do have a specific position set and insert them into the Rotors list, pushing unspecific positioned rotors backwards.
		For Each RotorNode As XmlNode In RotorNodes
			If Not IsNothing(RotorNode.Attributes.ItemOf("position")) Then
				Dim notches() As Char = RotorNode.Attributes.ItemOf("notches").Value.ToCharArray()
				Dim wiring As String = WiringFromXML(RotorNode)
				Dim position As Integer = Val(RotorNode.Attributes.ItemOf("position").Value) - 1
				Dim positionAttr = RotorNode.Attributes.ItemOf("position")
				Dim rotorID As String = If(positionAttr IsNot Nothing, "Rotor " & positionAttr.Value, "Unknown")
				Rotors.Insert(position, New Rotor(wiring, notches, rotorID))
			End If
		Next

		Return Rotors.ToArray()
	End Function


	Private Function WiringFromXML(ByRef ParentNode As XmlNode) As String
		' Build wiring string in A-Z order
		Dim wiringChars(25) As Char
		For Each MappingNode As XmlNode In ParentNode.ChildNodes
			Dim source As Char = MappingNode.Attributes.ItemOf("source").Value.ToUpper()(0)
			Dim destination As Char = MappingNode.Attributes.ItemOf("destination").Value.ToUpper()(0)
			Dim idx As Integer = Asc(source) - Asc("A")
			wiringChars(idx) = destination
		Next
		Return New String(wiringChars)
	End Function



End Module