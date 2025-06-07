Imports System.Xml

''' <summary>
''' Provides methods to construct Enigma rotors and reflector from XML data.
''' </summary>
Module MachineBuilder

	''' <summary>
	''' Helper to extract reflector wiring from an XmlDocument by reflector ID.
	''' </summary>
	''' <param name="XMLDoc">The reflector XML document.</param>
	''' <param name="reflectorID">The reflector ID (default "B").</param>
	''' <returns>Reflector wiring as a 26-character string.</returns>
	Private Function GetReflectorWiring(XMLDoc As XmlDocument, Optional reflectorID As String = "B") As String
		Dim reflectorNode As XmlNode = Nothing
		For Each node As XmlNode In XMLDoc.GetElementsByTagName("reflector")
			If node.Attributes IsNot Nothing AndAlso node.Attributes.ItemOf("id").Value = reflectorID Then
				reflectorNode = node
				Exit For
			End If
		Next
		If reflectorNode Is Nothing Then
			Throw New Exception("Reflector with id '" & reflectorID & "' not found.")
		End If
		Return reflectorNode.Attributes.ItemOf("wiring").Value.ToUpper()
	End Function

	''' <summary>
	''' Loads reflector wiring from an XML file path.
	''' </summary>
	''' <param name="Path">The file path to the reflector XML.</param>
	''' <returns>Reflector wiring as a 26-character string.</returns>
	Public Function ReflectorFromXMLFilePath(ByVal Path As String, Optional ByVal reflectorID As String = "B") As String
		Dim XMLDoc As New XmlDocument
		XMLDoc.Load(Path)
		Return GetReflectorWiring(XMLDoc, reflectorID)
	End Function

	''' <summary>
	''' Loads reflector wiring from raw XML data.
	''' </summary>
	''' <param name="XMLData">The reflector XML as a string.</param>
	''' <returns>Reflector wiring as a 26-character string.</returns>
	Public Function ReflectorFromXMLData(ByVal XMLData As String, Optional ByVal reflectorID As String = "B") As String
		Dim XMLDoc As New XmlDocument
		XMLDoc.LoadXml(XMLData)
		Return GetReflectorWiring(XMLDoc, reflectorID)
	End Function


	''' <summary>
	''' Loads reflector wiring for the specified reflector ID from an XML file path.
	''' </summary>
	''' <param name="XMLDoc">The file path to the reflector XML.</param>
	''' <param name="reflectorID">The reflector ID to load (default "B").</param>
	''' <returns>Reflector wiring as a 26-character string.</returns>
	Public Function ReflectorFromXMLDocument(ByRef XMLDoc As XmlDocument, Optional ByVal reflectorID As String = "B") As String

		Return GetReflectorWiring(XMLDoc, reflectorID)
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
		Dim RotorNodes As XmlNodeList = XMLDoc.GetElementsByTagName("rotor")
		Dim Rotors As New List(Of Rotor)
		For Each RotorNode As XmlNode In RotorNodes
			Dim idAttr = RotorNode.Attributes.ItemOf("id")
			Dim notchAttr = RotorNode.Attributes.ItemOf("notch")
			Dim wiringAttr = RotorNode.Attributes.ItemOf("wiring")
			Dim ringAttr = RotorNode.Attributes.ItemOf("ring")
			If idAttr Is Nothing OrElse notchAttr Is Nothing OrElse wiringAttr Is Nothing Then
				Throw New Exception("Rotor node missing required attribute.")
			End If
			Dim notches() As Char = notchAttr.Value.ToCharArray()
			Dim wiring As String = wiringAttr.Value.ToUpper()
			Dim rotorID As String = idAttr.Value
			Dim ringSetting As Integer = 0
			If ringAttr IsNot Nothing Then
				Integer.TryParse(ringAttr.Value, ringSetting)
			End If
			Rotors.Add(New Rotor(wiring, notches, rotorID, ringSetting))
		Next
		Return Rotors.ToArray()
	End Function


End Module