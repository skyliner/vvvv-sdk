using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;

using SlimDX;
using SlimDX.Direct3D9;
using VVVV.Core.Model;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

/// <summary>
/// Version 1 of the VVVV PluginInterface.
/// DirectX/SlimDX related parts are in a separate file: PluginDXInterface1.cs
///
/// To convert this to a typelib make sure AssemblyInfo.cs states: ComVisible(true).
/// Then on a commandline type:
/// <c>regasm _PluginInterfaces.dll /tlb</c>
/// This generates and registers the typelib which can then be imported e.g. via Delphi:Components:Import Component:Import Typelib
/// </summary>
namespace VVVV.PluginInterfaces.V1
{
	#region basic pins
	[Guid("19D25C40-AE80-4960-9847-4FECF661522B"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IConnectionHandler
	{
		bool Accepts([In, MarshalAs(UnmanagedType.IUnknown)] object source, [In, MarshalAs(UnmanagedType.IUnknown)] object sink);
		string GetFriendlyNameForSink([In, MarshalAs(UnmanagedType.IUnknown)] object sink);
		string GetFriendlyNameForSource([In, MarshalAs(UnmanagedType.IUnknown)] object source);
	}
	
	/// <summary>
	/// Base interface of all pin interfaces. Never used directly.
	/// </summary>
	[Guid("D3C5CB5C-C054-4AB6-AC04-6BDB34692B25"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown), 
	 SuppressUnmanagedCodeSecurity]
	public interface IPluginIO
	{
		/// <summary>
		/// The pins name.
		/// </summary>
		string Name{get; set;}
		/// <summary>
		/// The order property helps the node to arrange its pins visually. The higher the order, the more right the pin appears on the node.
		/// </summary>
		int Order{get; set;}
		/// <summary>
		/// Specifies whether the pin is connected in the patch or not.
		/// </summary>
		bool IsConnected{get;}
		/// <summary>
		/// Gets the plugin host which created this plugin io.
		/// </summary>
		IPluginHost PluginHost{get;}
	}
	
	/// <summary>
	/// Base interface of all ConfigurationPin interfaces. Never used directly.
	/// </summary>
	[Guid("11FDCEBD-FFC0-415D-90D5-DA4DBBDB5B67"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IPluginConfig: IPluginIO
	{
		/// <summary>
		/// The pins SliceCount specifies the number of Values (2D Vector, String...) it carries. This is like the length of an array or list.
		/// </summary>
		int SliceCount{get; set;}
		/// <summary>
		/// Returns a String of the pins concatenated Values. Typcally used internally only to save a pins state to disk.
		/// </summary>
		string SpreadAsString{get;}
		/// <summary>
		/// Returns whether any slice of this pin has been changed in the current frame. This information is typically used to determine if
		/// further processing is needed or can be ommited.
		/// </summary>
		bool PinIsChanged{get;}
	}
	
	/// <summary>
	/// Base interface of all InputPin interfaces. Never used directly.
	/// </summary>
	[Guid("68C6F37B-1D45-4683-9FC2-BC2580187D44"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IPluginIn: IPluginIO
	{
		/// <summary>
		/// The pins SliceCount specifies the number of Values (2D Vector, String...) it carries. This is like the length of an array or list.
		/// </summary>
		int SliceCount{get;}
		/// <summary>
		/// Returns a String of the pins concatenated Values. Typcally used internally only to save a pins state to disk.
		/// </summary>
		string SpreadAsString{get;}
		/// <summary>
		/// Returns whether any slice of this pin has been changed in the current frame. This information is typically used to determine if
		/// further processing is needed or can be ommited.
		/// </summary>
		bool PinIsChanged{get;}
		bool Validate();
		bool AutoValidate{get;set;}
	}
	
	/// <summary>
	/// Base interface of all fast InputPin interfaces. Never used directly.
	/// </summary>
	[Guid("9AFAD289-7C11-4296-B232-8B33FAC3E27D"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown), 
	 SuppressUnmanagedCodeSecurity]
	public interface IPluginFastIn: IPluginIO
	{
		/// <summary>
		/// The pins SliceCount specifies the number of Values (2D Vector, String...) it carries. This is like the length of an array or list.
		/// </summary>
		int SliceCount{get;}
		/// <summary>
		/// Returns a String of the pins concatenated Values. Typcally used internally only to save a pins state to disk.
		/// </summary>
		string SpreadAsString{get;}
		bool Validate();
		bool AutoValidate{get;set;}
	}
	
	/// <summary>
	/// Base interface of all OutputPin interfaces. Never used directly.
	/// </summary>
	[Guid("67FB9F25-0579-495C-8535-28CC15F54C55"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IPluginOut: IPluginIO
	{
		/// <summary>
		/// The pins SliceCount specifies the number of Values (2D Vector, String...) it carries. This is like the length of an array or list.
		/// </summary>
		int SliceCount{get; set;}
		/// <summary>
		/// Returns a String of the pins concatenated Values. Typcally used internally only to save a pins state to disk.
		/// </summary>
		string SpreadAsString{set;}
	}
	
	#endregion basic pins
	
	#region value pins

	/// <summary>
	/// Interface to a ConfigurationPin of type Value.
	/// </summary>
	[Guid("46154821-A76F-4258-846D-8524957F98D4"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	unsafe public interface IValueConfig: IPluginConfig
	{
		/// <summary>
		/// Used to write a Value to the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to write the Value to.</param>
		/// <param name="Value">The Value to write.</param>
		void SetValue(int Index, double Value);
		/// <summary>
		/// Used to write a 2D Vector to the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to write the 2D Vector to.</param>
		/// <param name="Value1">The Value to write to the 1st dimension.</param>
		/// <param name="Value2">The Value to write to the 2nd dimension.</param>
		void SetValue2D(int Index, double Value1, double Value2);
		/// <summary>
		/// Used to write a 3D Vector to the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to write the 3D Vector to.</param>
		/// <param name="Value1">The Value to write to the 1st dimension.</param>
		/// <param name="Value2">The Value to write to the 2nd dimension.</param>
		/// <param name="Value3">The Value to write to the 3rd dimension.</param>
		void SetValue3D(int Index, double Value1, double Value2, double Value3);
		/// <summary>
		/// Used to write a 4D Vector to the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to write the 4D Vector to.</param>
		/// <param name="Value1">The Value to write to the 1st dimension.</param>
		/// <param name="Value2">The Value to write to the 2nd dimension.</param>
		/// <param name="Value3">The Value to write to the 3rd dimension.</param>
		/// <param name="Value4">The Value to write to the 4th dimension.</param>
		void SetValue4D(int Index, double Value1, double Value2, double Value3, double Value4);
		/// <summary>
		/// Used to write a Matrix to the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to write the Matrix to.</param>
		/// <param name="Value">The Matrix to write.</param>
		void SetMatrix(int Index, Matrix4x4 Value);
		
		/// <summary>
		/// Used to retrieve a Value from the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to retrieve the Value from.</param>
		/// <param name="Value">The retrieved Value.</param>
		void GetValue(int Index, out double Value);
		/// <summary>
		/// Used to retrieve a 2D Vector from the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to retrieve the 2D Vector from.</param>
		/// <param name="Value1">The retrieved 1st dimension of the Vector.</param>
		/// <param name="Value2">The retrieved 2nd dimension of the Vector.</param>
		void GetValue2D(int Index, out double Value1, out double Value2);
		/// <summary>
		/// Used to retrieve a 3D Vector from the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to retrieve the 3D Vector from.</param>
		/// <param name="Value1">The retrieved 1st dimension of the Vector.</param>
		/// <param name="Value2">The retrieved 2nd dimension of the Vector.</param>
		/// <param name="Value3">The retrieved 3rd dimension of the Vector.</param>
		void GetValue3D(int Index, out double Value1, out double Value2, out double Value3);
		/// <summary>
		/// Used to retrieve a 4D Vector from the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to retrieve the 4D Vector from.</param>
		/// <param name="Value1">The retrieved 1st dimension of the Vector.</param>
		/// <param name="Value2">The retrieved 2nd dimension of the Vector.</param>
		/// <param name="Value3">The retrieved 3rd dimension of the Vector.</param>
		/// <param name="Value4">The retrieved 4th dimension of the Vector.</param>
		void GetValue4D(int Index, out double Value1, out double Value2, out double Value3, out double Value4);
		/// <summary>
		/// Used to retrieve a 4x4 Matrix from the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to retrieve the 4x4 Matrix from.</param>
		/// <param name="Value">The retrieved 4x4 Matrix.</param>
		void GetMatrix(int Index, out Matrix4x4 Value);
		/// <summary>
		/// Used to retrieve a Pointer to the Values of the pin.
		/// </summary>
		/// <param name="SliceCount">The pins current SliceCount, specifying the number of values accessible via the Pointer.</param>
		/// <param name="Value">A Pointer to the pins first Value.</param>
		void GetValuePointer(out int SliceCount, out double* Value);
		void GetValuePointer(out int* length, out double** dataPointer);

		/// <summary>
		/// Used to set the SubType of a Value pin, which is a set of limitations to the pins value range, used by the GUI to guide the user to insert correct values.
		/// Note though that this does not prevent a user from setting "wrong" values on a pin. Ultimately each node is responsible for dealing with all possible inputs correctly.
		/// Should only be called once immediately after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost(IPluginHost)">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="Min">Minimum of the Values range.</param>
		/// <param name="Max">Maximum of the Values range.</param>
		/// <param name="StepSize">StepSize used when scrolling the value up or down via the GUI.</param>
		/// <param name="Default">The Value the pin is initialized with and can be reset to at any time.</param>
		/// <param name="IsBang">Hint to the GUI that this Value is a bang.</param>
		/// <param name="IsToggle">Hint to the GUI that this is a toggling Value.</param>
		/// <param name="IsInteger">Hint to the GUI that this is an integer Value.</param>
		void SetSubType  (double Min, double Max, double StepSize, double Default, bool IsBang, bool IsToggle, bool IsInteger);
		/// <summary>
		/// Used to set the SubType of a 2D Vector pin, which is a set of limitations to the pins value range, used by the GUI to guide the user to insert correct values.
		/// Note though that this does not prevent a user from setting "wrong" values on a pin. Ultimately each node is responsible for dealing with all possible inputs correctly.
		/// Should only be called once immediately after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost(IPluginHost)">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="Min">Minimum of the Values range.</param>
		/// <param name="Max">Maximum of the Values range.</param>
		/// <param name="StepSize">StepSize used when scrolling the value up or down via the GUI.</param>
		/// <param name="Default1">The Value the pins 1st dimension is initialized with and can be reset to at any time.</param>
		/// <param name="Default2">The Value the pins 2nd dimension is initialized with and can be reset to at any time.</param>
		/// <param name="IsBang">Hint to the GUI that this Value is a bang.</param>
		/// <param name="IsToggle">Hint to the GUI that this is a toggling Value.</param>
		/// <param name="IsInteger">Hint to the GUI that this is an integer Value.</param>
		void SetSubType2D(double Min, double Max, double StepSize, double Default1, double Default2, bool IsBang, bool IsToggle, bool IsInteger);
		/// <summary>
		/// Used to set the SubType of a 3D Vector pin, which is a set of limitations to the pins value range, used by the GUI to guide the user to insert correct values.
		/// Note though that this does not prevent a user from setting "wrong" values on a pin. Ultimately each node is responsible for dealing with all possible inputs correctly.
		/// Should only be called once immediately after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost(IPluginHost)">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="Min">Minimum of the Values range.</param>
		/// <param name="Max">Maximum of the Values range.</param>
		/// <param name="StepSize">StepSize used when scrolling the value up or down via the GUI.</param>
		/// <param name="Default1">The Value the pins 1st dimension is initialized with and can be reset to at any time.</param>
		/// <param name="Default2">The Value the pins 2nd dimension is initialized with and can be reset to at any time.</param>
		/// <param name="Default3">The Value the pins 3rd dimension is initialized with and can be reset to at any time.</param>
		/// <param name="IsBang">Hint to the GUI that this Value is a bang.</param>
		/// <param name="IsToggle">Hint to the GUI that this is a toggling Value.</param>
		/// <param name="IsInteger">Hint to the GUI that this is an integer Value.</param>
		void SetSubType3D(double Min, double Max, double StepSize, double Default1, double Default2, double Default3, bool IsBang, bool IsToggle, bool IsInteger);
		/// <summary>
		/// Used to set the SubType of a 4D Vector pin, which is a set of limitations to the pins value range, used by the GUI to guide the user to insert correct values.
		/// Note though that this does not prevent a user from setting "wrong" values on a pin. Ultimately each node is responsible for dealing with all possible inputs correctly.
		/// Should only be called once immediately after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost(IPluginHost)">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="Min">Minimum of the Values range.</param>
		/// <param name="Max">Maximum of the Values range.</param>
		/// <param name="StepSize">StepSize used when scrolling the value up or down via the GUI.</param>
		/// <param name="Default1">The Value the pins 1st dimension is initialized with and can be reset to at any time.</param>
		/// <param name="Default2">The Value the pins 2nd dimension is initialized with and can be reset to at any time.</param>
		/// <param name="Default3">The Value the pins 3rd dimension is initialized with and can be reset to at any time.</param>
		/// <param name="Default4">The Value the pins 4th dimension is initialized with and can be reset to at any time.</param>
		/// <param name="IsBang">Hint to the GUI that this Value is a bang.</param>
		/// <param name="IsToggle">Hint to the GUI that this is a toggling Value.</param>
		/// <param name="IsInteger">Hint to the GUI that this is an integer Value.</param>
		void SetSubType4D(double Min, double Max, double StepSize, double Default1, double Default2, double Default3, double Default4, bool IsBang, bool IsToggle, bool IsInteger);
	}
	
	/// <summary>
	/// Interface to an InputPin of type Value.
	/// </summary>
	[Guid("40137258-9CDE-49F4-93BA-DE7D91007809"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	unsafe public interface IValueIn: IPluginIn				//value input pin
	{
		/// <summary>
		/// Used to retrieve a Value from the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to retrieve the Value from.</param>
		/// <param name="Value">The retrieved Value.</param>
		void GetValue(int Index, out double Value);
		/// <summary>
		/// Used to retrieve a 2D Vector from the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to retrieve the 2D Vector from.</param>
		/// <param name="Value1">The retrieved 1st dimension of the Vector.</param>
		/// <param name="Value2">The retrieved 2nd dimension of the Vector.</param>
		void GetValue2D(int Index, out double Value1, out double Value2);
		/// <summary>
		/// Used to retrieve a 3D Vector from the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to retrieve the 3D Vector from.</param>
		/// <param name="Value1">The retrieved 1st dimension of the Vector.</param>
		/// <param name="Value2">The retrieved 2nd dimension of the Vector.</param>
		/// <param name="Value3">The retrieved 3rd dimension of the Vector.</param>
		void GetValue3D(int Index, out double Value1, out double Value2, out double Value3);
		/// <summary>
		/// Used to retrieve a 4D Vector from the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to retrieve the 4D Vector from.</param>
		/// <param name="Value1">The retrieved 1st dimension of the Vector.</param>
		/// <param name="Value2">The retrieved 2nd dimension of the Vector.</param>
		/// <param name="Value3">The retrieved 3rd dimension of the Vector.</param>
		/// <param name="Value4">The retrieved 4th dimension of the Vector.</param>
		void GetValue4D(int Index, out double Value1, out double Value2, out double Value3, out double Value4);
		/// <summary>
		/// Used to retrieve a 4x4 Matrix from the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to retrieve the 4x4 Matrix from.</param>
		/// <param name="Value">The retrieved 4x4 Matrix.</param>
		void GetMatrix(int Index, out Matrix4x4 Value);
		/// <summary>
		/// Used to retrieve a Pointer to the Values of the pin, which can be used to retrive large Spreads of Values more efficiently.
		/// Attention: Don't use this Pointer to write Values to the pin!
		/// </summary>
		/// <param name="SliceCount">The pins current SliceCount, specifying the number of values accessible via the Pointer.</param>
		/// <param name="Value">A Pointer to the pins first Value.</param>
		void GetValuePointer(out int SliceCount, out double* Value);
		void GetValuePointer(out int* length, out double** dataPointer);
		
		/// <summary>
		/// Used to set the SubType of a Value pin, which is a set of limitations to the pins value range, used by the GUI to guide the user to insert correct values.
		/// Note though that this does not prevent a user from setting "wrong" values on a pin. Ultimately each node is responsible for dealing with all possible inputs correctly.
		/// Should only be called once immediately after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost(IPluginHost)">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="Min">Minimum of the Values range.</param>
		/// <param name="Max">Maximum of the Values range.</param>
		/// <param name="StepSize">StepSize used when scrolling the value up or down via the GUI.</param>
		/// <param name="Default">The Value the pin is initialized with and can be reset to at any time.</param>
		/// <param name="IsBang">Hint to the GUI that this Value is a bang.</param>
		/// <param name="IsToggle">Hint to the GUI that this is a toggling Value.</param>
		/// <param name="IsInteger">Hint to the GUI that this is an integer Value.</param>
		void SetSubType  (double Min, double Max, double StepSize, double Default, bool IsBang, bool IsToggle, bool IsInteger);
		/// <summary>
		/// Used to set the SubType of a 2D Vector pin, which is a set of limitations to the pins value range, used by the GUI to guide the user to insert correct values.
		/// Note though that this does not prevent a user from setting "wrong" values on a pin. Ultimately each node is responsible for dealing with all possible inputs correctly.
		/// Should only be called once immediately after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost(IPluginHost)">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="Min">Minimum of the Values range.</param>
		/// <param name="Max">Maximum of the Values range.</param>
		/// <param name="StepSize">StepSize used when scrolling the value up or down via the GUI.</param>
		/// <param name="Default1">The Value the pins 1st dimension is initialized with and can be reset to at any time.</param>
		/// <param name="Default2">The Value the pins 2nd dimension is initialized with and can be reset to at any time.</param>
		/// <param name="IsBang">Hint to the GUI that this Value is a bang.</param>
		/// <param name="IsToggle">Hint to the GUI that this is a toggling Value.</param>
		/// <param name="IsInteger">Hint to the GUI that this is an integer Value.</param>
		void SetSubType2D(double Min, double Max, double StepSize, double Default1, double Default2, bool IsBang, bool IsToggle, bool IsInteger);
		/// <summary>
		/// Used to set the SubType of a 3D Vector pin, which is a set of limitations to the pins value range, used by the GUI to guide the user to insert correct values.
		/// Note though that this does not prevent a user from setting "wrong" values on a pin. Ultimately each node is responsible for dealing with all possible inputs correctly.
		/// Should only be called once immediately after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost(IPluginHost)">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="Min">Minimum of the Values range.</param>
		/// <param name="Max">Maximum of the Values range.</param>
		/// <param name="StepSize">StepSize used when scrolling the value up or down via the GUI.</param>
		/// <param name="Default1">The Value the pins 1st dimension is initialized with and can be reset to at any time.</param>
		/// <param name="Default2">The Value the pins 2nd dimension is initialized with and can be reset to at any time.</param>
		/// <param name="Default3">The Value the pins 3rd dimension is initialized with and can be reset to at any time.</param>
		/// <param name="IsBang">Hint to the GUI that this Value is a bang.</param>
		/// <param name="IsToggle">Hint to the GUI that this is a toggling Value.</param>
		/// <param name="IsInteger">Hint to the GUI that this is an integer Value.</param>
		void SetSubType3D(double Min, double Max, double StepSize, double Default1, double Default2, double Default3, bool IsBang, bool IsToggle, bool IsInteger);
		/// <summary>
		/// Used to set the SubType of a 4D Vector pin, which is a set of limitations to the pins value range, used by the GUI to guide the user to insert correct values.
		/// Note though that this does not prevent a user from setting "wrong" values on a pin. Ultimately each node is responsible for dealing with all possible inputs correctly.
		/// Should only be called once immediately after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost(IPluginHost)">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="Min">Minimum of the Values range.</param>
		/// <param name="Max">Maximum of the Values range.</param>
		/// <param name="StepSize">StepSize used when scrolling the value up or down via the GUI.</param>
		/// <param name="Default1">The Value the pins 1st dimension is initialized with and can be reset to at any time.</param>
		/// <param name="Default2">The Value the pins 2nd dimension is initialized with and can be reset to at any time.</param>
		/// <param name="Default3">The Value the pins 3rd dimension is initialized with and can be reset to at any time.</param>
		/// <param name="Default4">The Value the pins 4th dimension is initialized with and can be reset to at any time.</param>
		/// <param name="IsBang">Hint to the GUI that this Value is a bang.</param>
		/// <param name="IsToggle">Hint to the GUI that this is a toggling Value.</param>
		/// <param name="IsInteger">Hint to the GUI that this is an integer Value.</param>
		void SetSubType4D(double Min, double Max, double StepSize, double Default1, double Default2, double Default3, double Default4, bool IsBang, bool IsToggle, bool IsInteger);
	}

	/// <summary>
	/// Interface to a fast InputPin of type Value.
	/// </summary>
	[Guid("095081B7-D929-4459-83C0-18AA809E6635"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown), 
	 SuppressUnmanagedCodeSecurity]
	unsafe public interface IValueFastIn: IPluginFastIn		//fast value input pin
	{
		/// <summary>
		/// Used to retrieve a Value from the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to retrieve the Value from.</param>
		/// <param name="Value">The retrieved Value.</param>
		void GetValue(int Index, out double Value);
		/// <summary>
		/// Used to retrieve a 2D Vector from the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to retrieve the 2D Vector from.</param>
		/// <param name="Value1">The retrieved 1st dimension of the Vector.</param>
		/// <param name="Value2">The retrieved 2nd dimension of the Vector.</param>
		void GetValue2D(int Index, out double Value1, out double Value2);
		/// <summary>
		/// Used to retrieve a 3D Vector from the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to retrieve the 3D Vector from.</param>
		/// <param name="Value1">The retrieved 1st dimension of the Vector.</param>
		/// <param name="Value2">The retrieved 2nd dimension of the Vector.</param>
		/// <param name="Value3">The retrieved 3rd dimension of the Vector.</param>
		void GetValue3D(int Index, out double Value1, out double Value2, out double Value3);
		/// <summary>
		/// Used to retrieve a 4D Vector from the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to retrieve the 4D Vector from.</param>
		/// <param name="Value1">The retrieved 1st dimension of the Vector.</param>
		/// <param name="Value2">The retrieved 2nd dimension of the Vector.</param>
		/// <param name="Value3">The retrieved 3rd dimension of the Vector.</param>
		/// <param name="Value4">The retrieved 4th dimension of the Vector.</param>
		void GetValue4D(int Index, out double Value1, out double Value2, out double Value3, out double Value4);
		/// <summary>
		/// Used to retrieve a 4x4 Matrix from the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to retrieve the 4x4 Matrix from.</param>
		/// <param name="Value">The retrieved 4x4 Matrix.</param>
		void GetMatrix(int Index, out Matrix4x4 Value);
		/// <summary>
		/// Used to retrieve a Pointer to the Values of the pin, which can be used to retrive large Spreads of Values more efficiently.
		/// Attention: Don't use this Pointer to write Values to the pin!
		/// </summary>
		/// <param name="SliceCount">The pins current SliceCount, specifying the number of values accessible via the Pointer.</param>
		/// <param name="Value">A Pointer to the pins first Value.</param>
		void GetValuePointer(out int SliceCount, out double* Value);
		void GetValuePointer(out int* length, out double** dataPointer);
		
		/// <summary>
		/// Used to set the SubType of a Value pin, which is a set of limitations to the pins value range, used by the GUI to guide the user to insert correct values.
		/// Note though that this does not prevent a user from setting "wrong" values on a pin. Ultimately each node is responsible for dealing with all possible inputs correctly.
		/// Should only be called once immediately after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost(IPluginHost)">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="Min">Minimum of the Values range.</param>
		/// <param name="Max">Maximum of the Values range.</param>
		/// <param name="StepSize">StepSize used when scrolling the value up or down via the GUI.</param>
		/// <param name="Default">The Value the pin is initialized with and can be reset to at any time.</param>
		/// <param name="IsBang">Hint to the GUI that this Value is a bang.</param>
		/// <param name="IsToggle">Hint to the GUI that this is a toggling Value.</param>
		/// <param name="IsInteger">Hint to the GUI that this is an integer Value.</param>
		void SetSubType  (double Min, double Max, double StepSize, double Default, bool IsBang, bool IsToggle, bool IsInteger);
		/// <summary>
		/// Used to set the SubType of a 2D Vector pin, which is a set of limitations to the pins value range, used by the GUI to guide the user to insert correct values.
		/// Note though that this does not prevent a user from setting "wrong" values on a pin. Ultimately each node is responsible for dealing with all possible inputs correctly.
		/// Should only be called once immediately after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost(IPluginHost)">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="Min">Minimum of the Values range.</param>
		/// <param name="Max">Maximum of the Values range.</param>
		/// <param name="StepSize">StepSize used when scrolling the value up or down via the GUI.</param>
		/// <param name="Default1">The Value the pins 1st dimension is initialized with and can be reset to at any time.</param>
		/// <param name="Default2">The Value the pins 2nd dimension is initialized with and can be reset to at any time.</param>
		/// <param name="IsBang">Hint to the GUI that this Value is a bang.</param>
		/// <param name="IsToggle">Hint to the GUI that this is a toggling Value.</param>
		/// <param name="IsInteger">Hint to the GUI that this is an integer Value.</param>
		void SetSubType2D(double Min, double Max, double StepSize, double Default1, double Default2, bool IsBang, bool IsToggle, bool IsInteger);
		/// <summary>
		/// Used to set the SubType of a 3D Vector pin, which is a set of limitations to the pins value range, used by the GUI to guide the user to insert correct values.
		/// Note though that this does not prevent a user from setting "wrong" values on a pin. Ultimately each node is responsible for dealing with all possible inputs correctly.
		/// Should only be called once immediately after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost(IPluginHost)">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="Min">Minimum of the Values range.</param>
		/// <param name="Max">Maximum of the Values range.</param>
		/// <param name="StepSize">StepSize used when scrolling the value up or down via the GUI.</param>
		/// <param name="Default1">The Value the pins 1st dimension is initialized with and can be reset to at any time.</param>
		/// <param name="Default2">The Value the pins 2nd dimension is initialized with and can be reset to at any time.</param>
		/// <param name="Default3">The Value the pins 3rd dimension is initialized with and can be reset to at any time.</param>
		/// <param name="IsBang">Hint to the GUI that this Value is a bang.</param>
		/// <param name="IsToggle">Hint to the GUI that this is a toggling Value.</param>
		/// <param name="IsInteger">Hint to the GUI that this is an integer Value.</param>
		void SetSubType3D(double Min, double Max, double StepSize, double Default1, double Default2, double Default3, bool IsBang, bool IsToggle, bool IsInteger);
		/// <summary>
		/// Used to set the SubType of a 4D Vector pin, which is a set of limitations to the pins value range, used by the GUI to guide the user to insert correct values.
		/// Note though that this does not prevent a user from setting "wrong" values on a pin. Ultimately each node is responsible for dealing with all possible inputs correctly.
		/// Should only be called once immediately after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost(IPluginHost)">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="Min">Minimum of the Values range.</param>
		/// <param name="Max">Maximum of the Values range.</param>
		/// <param name="StepSize">StepSize used when scrolling the value up or down via the GUI.</param>
		/// <param name="Default1">The Value the pins 1st dimension is initialized with and can be reset to at any time.</param>
		/// <param name="Default2">The Value the pins 2nd dimension is initialized with and can be reset to at any time.</param>
		/// <param name="Default3">The Value the pins 3rd dimension is initialized with and can be reset to at any time.</param>
		/// <param name="Default4">The Value the pins 4th dimension is initialized with and can be reset to at any time.</param>
		/// <param name="IsBang">Hint to the GUI that this Value is a bang.</param>
		/// <param name="IsToggle">Hint to the GUI that this is a toggling Value.</param>
		/// <param name="IsInteger">Hint to the GUI that this is an integer Value.</param>
		void SetSubType4D(double Min, double Max, double StepSize, double Default1, double Default2, double Default3, double Default4, bool IsBang, bool IsToggle, bool IsInteger);
	}
	
	/// <summary>
	/// Interface to an OutputPin of type Value.
	/// </summary>
	[Guid("B55B70E8-9C3D-408D-B9F9-A90CF8288FC7"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown), 
	 SuppressUnmanagedCodeSecurity]
	unsafe public interface IValueOut: IPluginOut			//value output pin
	{
		/// <summary>
		/// Used to write a Value to the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to write the Value to.</param>
		/// <param name="Value">The Value to write.</param>
		void SetValue(int Index, double Value);
		/// <summary>
		/// Used to write a 2D Vector to the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to write the 2D Vector to.</param>
		/// <param name="Value1">The Value to write to the 1st dimension.</param>
		/// <param name="Value2">The Value to write to the 2nd dimension.</param>
		void SetValue2D(int Index, double Value1, double Value2);
		/// <summary>
		/// Used to write a 3D Vector to the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to write the 3D Vector to.</param>
		/// <param name="Value1">The Value to write to the 1st dimension.</param>
		/// <param name="Value2">The Value to write to the 2nd dimension.</param>
		/// <param name="Value3">The Value to write to the 3rd dimension.</param>
		void SetValue3D(int Index, double Value1, double Value2, double Value3);
		/// <summary>
		/// Used to write a 4D Vector to the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to write the 4D Vector to.</param>
		/// <param name="Value1">The Value to write to the 1st dimension.</param>
		/// <param name="Value2">The Value to write to the 2nd dimension.</param>
		/// <param name="Value3">The Value to write to the 3rd dimension.</param>
		/// <param name="Value4">The Value to write to the 4th dimension.</param>
		void SetValue4D(int Index, double Value1, double Value2, double Value3, double Value4);
		/// <summary>
		/// Used to write a Matrix to the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to write the Matrix to.</param>
		/// <param name="Value">The Matrix to write.</param>
		void SetMatrix(int Index, Matrix4x4 Value);
		/// <summary>
		/// Used to retrieve a Pointer to the Values of the pin, which can be used to write large number of values more efficiently.
		/// Note though, that when writing Values to the Pointer the pins dimensions and overall SliceCount have to be taken care of manually.
		/// </summary>
		/// <param name="Value">A Pointer to the pins first Value.</param>
		void GetValuePointer(out double* Value);
		void GetValuePointer(out double** Value);

		/// <summary>
		/// Used to set the SubType of a Value pin, which is a set of limitations to the pins value range, used by the GUI to guide the user to insert correct values.
		/// Note though that this does not prevent a user from setting "wrong" values on a pin. Ultimately each node is responsible for dealing with all possible inputs correctly.
		/// Should only be called once immediately after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost(IPluginHost)">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="Min">Minimum of the Values range.</param>
		/// <param name="Max">Maximum of the Values range.</param>
		/// <param name="StepSize">StepSize used when scrolling the value up or down via the GUI.</param>
		/// <param name="Default">The Value the pin is initialized with and can be reset to at any time.</param>
		/// <param name="IsBang">Hint to the GUI that this Value is a bang.</param>
		/// <param name="IsToggle">Hint to the GUI that this is a toggling Value.</param>
		/// <param name="IsInteger">Hint to the GUI that this is an integer Value.</param>
		void SetSubType  (double Min, double Max, double StepSize, double Default, bool IsBang, bool IsToggle, bool IsInteger);
		/// <summary>
		/// Used to set the SubType of a 2D Vector pin, which is a set of limitations to the pins value range, used by the GUI to guide the user to insert correct values.
		/// Note though that this does not prevent a user from setting "wrong" values on a pin. Ultimately each node is responsible for dealing with all possible inputs correctly.
		/// Should only be called once immediately after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost(IPluginHost)">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="Min">Minimum of the Values range.</param>
		/// <param name="Max">Maximum of the Values range.</param>
		/// <param name="StepSize">StepSize used when scrolling the value up or down via the GUI.</param>
		/// <param name="Default1">The Value the pins 1st dimension is initialized with and can be reset to at any time.</param>
		/// <param name="Default2">The Value the pins 2nd dimension is initialized with and can be reset to at any time.</param>
		/// <param name="IsBang">Hint to the GUI that this Value is a bang.</param>
		/// <param name="IsToggle">Hint to the GUI that this is a toggling Value.</param>
		/// <param name="IsInteger">Hint to the GUI that this is an integer Value.</param>
		void SetSubType2D(double Min, double Max, double StepSize, double Default1, double Default2, bool IsBang, bool IsToggle, bool IsInteger);
		/// <summary>
		/// Used to set the SubType of a 3D Vector pin, which is a set of limitations to the pins value range, used by the GUI to guide the user to insert correct values.
		/// Note though that this does not prevent a user from setting "wrong" values on a pin. Ultimately each node is responsible for dealing with all possible inputs correctly.
		/// Should only be called once immediately after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost(IPluginHost)">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="Min">Minimum of the Values range.</param>
		/// <param name="Max">Maximum of the Values range.</param>
		/// <param name="StepSize">StepSize used when scrolling the value up or down via the GUI.</param>
		/// <param name="Default1">The Value the pins 1st dimension is initialized with and can be reset to at any time.</param>
		/// <param name="Default2">The Value the pins 2nd dimension is initialized with and can be reset to at any time.</param>
		/// <param name="Default3">The Value the pins 3rd dimension is initialized with and can be reset to at any time.</param>
		/// <param name="IsBang">Hint to the GUI that this Value is a bang.</param>
		/// <param name="IsToggle">Hint to the GUI that this is a toggling Value.</param>
		/// <param name="IsInteger">Hint to the GUI that this is an integer Value.</param>
		void SetSubType3D(double Min, double Max, double StepSize, double Default1, double Default2, double Default3, bool IsBang, bool IsToggle, bool IsInteger);
		/// <summary>
		/// Used to set the SubType of a 4D Vector pin, which is a set of limitations to the pins value range, used by the GUI to guide the user to insert correct values.
		/// Note though that this does not prevent a user from setting "wrong" values on a pin. Ultimately each node is responsible for dealing with all possible inputs correctly.
		/// Should only be called once immediately after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost(IPluginHost)">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="Min">Minimum of the Values range.</param>
		/// <param name="Max">Maximum of the Values range.</param>
		/// <param name="StepSize">StepSize used when scrolling the value up or down via the GUI.</param>
		/// <param name="Default1">The Value the pins 1st dimension is initialized with and can be reset to at any time.</param>
		/// <param name="Default2">The Value the pins 2nd dimension is initialized with and can be reset to at any time.</param>
		/// <param name="Default3">The Value the pins 3rd dimension is initialized with and can be reset to at any time.</param>
		/// <param name="Default4">The Value the pins 4th dimension is initialized with and can be reset to at any time.</param>
		/// <param name="IsBang">Hint to the GUI that this Value is a bang.</param>
		/// <param name="IsToggle">Hint to the GUI that this is a toggling Value.</param>
		/// <param name="IsInteger">Hint to the GUI that this is an integer Value.</param>
		void SetSubType4D(double Min, double Max, double StepSize, double Default1, double Default2, double Default3, double Default4, bool IsBang, bool IsToggle, bool IsInteger);
	}
	
	#endregion value pins
	
	#region string pins
	
	/// <summary>
	/// Interface to a ConfigurationPin of type String.
	/// </summary>
	[Guid("1FF25AD1-FBAB-4B29-8BAC-82CE53135868"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IStringConfig: IPluginConfig
	{
		/// <summary>
		/// Used to write a String to the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to write the String to.</param>
		/// <param name="Value">The String to write.</param>
		void SetString(int Index, string Value);
		/// <summary>
		/// Used to retrieve a String from the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to retrieve the String from.</param>
		/// <param name="Value">The retrieved String.</param>
		void GetString(int Index, out string Value);
		/// <summary>
		/// Used to set the SubType of a String pin, which is a more detailed specification of the String, used by the GUI to guide the user to insert correct Strings.
		/// Note though that this does not prevent a user from setting "wrong" Strings on a pin. Ultimately each node is responsible for dealing with all possible inputs correctly.
		/// Should only be called once immediately after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost(IPluginHost)">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="Default">The String the pin is initialized with and can be reset to at any time.</param>
		/// <param name="IsFilename">Hint to the GUI that this String is a filename</param>
		void SetSubType(string Default, bool IsFilename);
		/// <summary>
		/// Alternative version to <see cref="SetSubType(string, bool)">IStringConfig.SetSubType()</see> with more options.
		/// </summary>
		/// <param name="Default">The String the pin is initialized with and can be reset to at any time</param>
		/// <param name="MaxCharacters">Constrains the string to a given number of characters. Use -1 for unlimited characters</param>
		/// <param name="FileMask">Filemask in the form of: Audio File (*.wav, *.mp3)|*.wav;*.mp3</param>
		/// <param name="StringType">Enum specifying the type of string more precisely.</param>
		void SetSubType2(string Default, int MaxCharacters, string FileMask, TStringType StringType);
	}
	
	/// <summary>
	/// Interface to an InputPin of type String.
	/// </summary>
	[Guid("E329D418-20DE-4D91-B060-60EF2D73A7A6"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IStringIn: IPluginIn
	{
		/// <summary>
		/// Used to retrieve a String from the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to retrieve the String from.</param>
		/// <param name="Value">The retrieved String.</param>
		void GetString(int Index, out string Value);
		/// <summary>
		/// Used to set the SubType of a String pin, which is a more detailed specification of the String, used by the GUI to guide the user to insert correct values.
		/// Note though that this does not prevent a user from setting "wrong" Strings on a pin. Ultimately each node is responsible for dealing with all possible inputs correctly.
		/// Should only be called once immediately after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost(IPluginHost)">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="Default">The String the pin is initialized with and can be reset to at any time.</param>
		/// <param name="IsFilename">Hint to the GUI that this String is a filename</param>
		void SetSubType(string Default, bool IsFilename);
		/// <summary>
		/// Alternative version to <see cref="SetSubType(string, bool)">IStringIn.SetSubType()</see> with more options.
		/// </summary>
		/// <param name="Default">The String the pin is initialized with and can be reset to at any time</param>
		/// <param name="MaxCharacters">Constrains the string to a given number of characters</param>
		/// <param name="FileMask">Filemask in the form of: Audio File (*.wav, *.mp3)|*.wav;*.mp3</param>
		/// <param name="StringType">Enum specifying the type of string more precisely.</param>
		void SetSubType2(string Default, int MaxCharacters, string FileMask, TStringType StringType);
	}

	/// <summary>
	/// Interface to an OutputPin of type String.
	/// </summary>
	[Guid("EC32C616-A85F-42AC-B7D1-630E1F739D1D"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IStringOut: IPluginOut
	{
		/// <summary>
		/// Used to write a String to the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to write the String to.</param>
		/// <param name="Value">The String to write.</param>
		void SetString(int Index, string Value);
		/// <summary>
		/// Used to set the SubType of a String pin, which is a more detailed specification of the String, used by the GUI to guide the user to insert correct values.
		/// Note though that this does not prevent a user from setting "wrong" Strings on a pin. Ultimately each node is responsible for dealing with all possible inputs correctly.
		/// Should only be called once immediately after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost(IPluginHost)">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="Default">The String the pin is initialized with and can be reset to at any time.</param>
		/// <param name="IsFilename">Hint to the GUI that this String is a filename</param>
		void SetSubType(string Default, bool IsFilename);
		/// <summary>
		/// Alternative version to <see cref="SetSubType(string, bool)">IStringOut.SetSubType()</see> with more options.
		/// </summary>
		/// <param name="Default">The String the pin is initialized with and can be reset to at any time</param>
		/// <param name="MaxCharacters">Constrains the string to a given number of characters</param>
		/// <param name="FileMask">Filemask in the form of: Audio File (*.wav, *.mp3)|*.wav;*.mp3</param>
		/// <param name="StringType">Enum specifying the type of string more precisely.</param>
		void SetSubType2(string Default, int MaxCharacters, string FileMask, TStringType StringType);
	}
	
	#endregion string pins
	
	#region color pins
	
	/// <summary>
	/// Interface to a ConfigurationPin of type Color.
	/// </summary>
	[Guid("BAA49637-29FA-426A-9188-86906E660D30"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	unsafe public interface IColorConfig: IPluginConfig
	{
		/// <summary>
		/// Used to write a Color to the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to write the Color to.</param>
		/// <param name="Color">The Color to write.</param>
		void SetColor(int Index, RGBAColor Color);
		/// <summary>
		/// Used to retrieve a Color from the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to retrieve the Color from.</param>
		/// <param name="Color">The retrieved Color.</param>
		void GetColor(int Index, out RGBAColor Color);
		/// <summary>
		/// Used to retrieve a Pointer to the Colors of the pin, which can be used to retrive large Spreads of Colors more efficiently.
		/// Each Color consists of 4 doubles, one for each of Red, Green, Blue and Alpha.
		/// Attention: Don't use this Pointer to write Colors to the pin!
		/// </summary>
		/// <param name="SliceCount">The pins current SliceCount, specifying the number of colors accessible via the Pointer.</param>
		/// <param name="Value">A Pointer to the pins first Colors Red channel double.</param>
		
		void GetColorPointer(out int SliceCount, out double* Value);
		void GetColorPointer(out int* pLength, out double** ppData);

		/// <summary>
		/// Used to set the SubType of a Color pin, which is a more detailed specification of the Color, used by the GUI to guide the user to insert correct Colors.
		/// Note though that this does not prevent a user from setting "wrong" Colors on a pin. Ultimately each node is responsible for dealing with all possible inputs correctly.
		/// Should only be called once immediately after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost(IPluginHost)">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="Default">The Color the pin is initialized with and can be reset to at any time.</param>
		/// <param name="HasAlpha">Hint to the GUI that this Color has an alpha channel.</param>
		void SetSubType(RGBAColor Default, bool HasAlpha);
	}
	
	/// <summary>
	/// Interface to an InputPin of type Color.
	/// </summary>
	[Guid("CB6289A8-28BD-4A52-9B7A-BC1092EA2FA5"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	unsafe public interface IColorIn: IPluginIn
	{
		/// <summary>
		/// Used to retrieve a Color from the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to retrieve the Color from.</param>
		/// <param name="Color">The retrieved Color.</param>
		void GetColor(int Index, out RGBAColor Color);
		/// <summary>
		/// Used to retrieve a Pointer to the Colors of the pin, which can be used to retrive large Spreads of Colors more efficiently.
		/// Each Color consists of 4 doubles, one for each of Red, Green, Blue and Alpha.
		/// Attention: Don't use this Pointer to write Colors to the pin!
		/// </summary>
		/// <param name="SliceCount">The pins current SliceCount, specifying the number of colors accessible via the Pointer.</param>
		/// <param name="Value">A Pointer to the pins first Colors Red channel double.</param>
		void GetColorPointer(out int SliceCount, out double* Value);
		void GetColorPointer(out int* pLength, out double** ppData);
		/// <summary>
		/// Used to set the SubType of a Color pin, which is a more detailed specification of the Color, used by the GUI to guide the user to insert correct Colors.
		/// Note though that this does not prevent a user from setting "wrong" Colors on a pin. Ultimately each node is responsible for dealing with all possible inputs correctly.
		/// Should only be called once immediately after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost(IPluginHost)">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="Default">The Color the pin is initialized with and can be reset to at any time.</param>
		/// <param name="HasAlpha">Hint to the GUI that this Color has an alpha channel.</param>
		void SetSubType(RGBAColor Default, bool HasAlpha);
	}
	
	/// <summary>
	/// Interface to an OutputPin of type Color.
	/// </summary>
	[Guid("432CE6BA-6F57-4387-A223-D2DAFA8125F0"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	unsafe public interface IColorOut: IPluginOut
	{
		/// <summary>
		/// Used to write a Color to the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to write the Color to.</param>
		/// <param name="Color">The Color to write.</param>
		void SetColor(int Index, RGBAColor Color);
		/// <summary>
		/// Used to retrieve a Pointer to the Colors of the pin, which can be used to write large number of Colors more efficiently.
		/// Each Color consists of 4 doubles, one for each of Red, Green, Blue and Alpha.
		/// Note though, that when writing Colors to the Pointer the pins SliceCount has to be taken care of manually.
		/// </summary>
		/// <param name="Value">A Pointer to the pins first Colors Red channel double.</param>
		void GetColorPointer(out double* Value);
		/// <summary>
		/// Used to set the SubType of a Color pin, which is a more detailed specification of the Color, used by the GUI to guide the user to insert correct Colors.
		/// Note though that this does not prevent a user from setting "wrong" Colors on a pin. Ultimately each node is responsible for dealing with all possible inputs correctly.
		/// Should only be called once immediately after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost(IPluginHost)">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="Default">The Color the pin is initialized with and can be reset to at any time.</param>
		/// <param name="HasAlpha">Hint to the GUI that this Color has an alpha channel.</param>
		void SetSubType(RGBAColor Default, bool HasAlpha);
		void GetColorPointer(out double** ppDst);
	}
	
	#endregion color pins
	
	#region enum pins
	/// <summary>
	/// Interface to a ConfigurationPin of type Enum.
	/// </summary>
	[Guid("2FE17270-7B4C-4A46-A4EB-E8B56B9AD197"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IEnumConfig: IPluginConfig
	{
		/// <summary>
		/// Used to write an Enum given as an ordinal value to the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to write the Enum to.</param>
		/// <param name="Value">The ordinal Enum value to write.</param>
		void SetOrd(int Index, int Value);
		/// <summary>
		/// Used to write an Enum given as an string value to the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to write the Enum to.</param>
		/// <param name="Value">The string Enum value to write.</param>
		void SetString(int Index, string Value);
		/// <summary>
		/// Used to retrieve an Enum in ordinal form from the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to retrieve the Enum from.</param>
		/// <param name="Value">The retrieved Enum.</param>
		void GetOrd(int Index, out int Value);
		/// <summary>
		/// Used to retrieve an Enum in string form from the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to retrieve the Enum from.</param>
		/// <param name="Value">The retrieved Enum.</param>
		void GetString(int Index, out string Value);
		/// <summary>
		/// Used to set the SubType of an Enum pin. Should only be called once immediately 
		/// after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost(IPluginHost)">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="EnumName">Name of the Enum type to set to the pin. If the given name 
		/// is not yet registered with vvvv a new type with this name is created. 
		/// Using <see cref="VVVV.PluginInterfaces.V1.IPluginHost.UpdateEnum">IPluginHost.UpdateEnum</see> 
		/// a newly created Enum can be filled with custom entries.</param>
		void SetSubType(string EnumName);
	}
	
	/// <summary>
	/// Interface to an InputPin of type Enum.
	/// </summary>
	[Guid("DE852C36-FE3A-4767-97F5-7595A9A59D6A"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IEnumIn: IPluginIn
	{
		/// <summary>
		/// Used to retrieve an Enum in ordinal form from the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to retrieve the Enum from.</param>
		/// <param name="Value">The retrieved Enum.</param>
		void GetOrd(int Index, out int Value);
		/// <summary>
		/// Used to retrieve an Enum in string form from the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to retrieve the Enum from.</param>
		/// <param name="Value">The retrieved Enum.</param>
		void GetString(int Index, out string Value);
		/// <summary>
		/// Used to set the SubType of an Enum pin. Should only be called once immediately 
		/// after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost(IPluginHost)">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="EnumName">Name of the Enum type to set to the pin. If the given name 
		/// is not yet registered with vvvv a new type with this name is created. 
		/// Using <see cref="VVVV.PluginInterfaces.V1.IPluginHost.UpdateEnum">IPluginHost.UpdateEnum</see> 
		/// a newly created Enum can be filled with custom entries.</param>
		void SetSubType(string EnumName);
	}

	/// <summary>
	/// Interface to an OutputPin of type Enum.
	/// </summary>
	[Guid("C933059A-C46E-4149-966D-04D03B93A078"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IEnumOut: IPluginOut
	{
		/// <summary>
		/// Used to write an Enum given as an ordinal value to the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to write the Enum to.</param>
		/// <param name="Value">The ordinal Enum value to write.</param>
		void SetOrd(int Index, int Value);
		/// <summary>
		/// Used to write an Enum given as an string value to the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to write the Enum to.</param>
		/// <param name="Value">The string Enum value to write.</param>
		void SetString(int Index, string Value);
		/// <summary>
		/// Used to set the SubType of an Enum pin. Should only be called once immediately 
		/// after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost(IPluginHost)">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="EnumName">Name of the Enum type to set to the pin. If the given name 
		/// is not yet registered with vvvv a new type with this name is created. 
		/// Using <see cref="VVVV.PluginInterfaces.V1.IPluginHost.UpdateEnum">IPluginHost.UpdateEnum</see> 
		/// a newly created Enum can be filled with custom entries.</param>
		void SetSubType(string EnumName);
	}
	#endregion enum pins
	
	#region node pins
	/// <summary>
	/// Base Interface for NodePin connections
	/// </summary>
	[Guid("AB312E34-8025-40F2-8241-1958793F3D39"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Obsolete("Not needed anymore in beta>26.")]
	public interface INodeIOBase
	{}
	
	/// <summary>
	/// Interface to an InputPin of the generic node type
	/// </summary>
	[Guid("FE6FEBC6-8581-4EB5-9AC8-E428CB9D1A03"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface INodeIn: IPluginIn
	{
		/// <summary>
		/// Used to retrieve the actual slice index this pin has to access on the upstream node. Note that the actual slice
		/// index maybe convoluted by an upstream node like GetSlice (node).
		/// </summary>
		/// <param name="slice">The slice index as seen by this pin.</param>
		/// <param name="UpstreamSlice">The actual slice index as probably convoluted via upstream GetSlice (node).</param>
		void GetUpsreamSlice(int slice, out int UpstreamSlice);
		/// <summary>
		/// Used to retrieve a reference of an interface offered by the upstream connected node.
		/// </summary>
		/// <param name="UpstreamInterface">The retrieved interface.</param>
		[Obsolete("Replaced by GetUpstreamInterface(object UpstreamInterface).")]
		void GetUpstreamInterface(out INodeIOBase UpstreamInterface);
		/// <summary>
		/// Used to set the SubType of a node pin, which is a more detailed specification of the node type via a set of Guids that identifiy the interfaces accepted on this pin.
		/// The SubType is used by the GUI to guide the user to make only links between pins that understand the same interfaces.
		/// Should only be called once immediately after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost(IPluginHost)">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="Guids">An array of Guids (typically only one) that specifies the interfaces that this input accepts.</param>
		/// <param name="FriendlyName">A user readable name specifying the type of the node connection.</param>
		void SetSubType(Guid[] Guids, string FriendlyName);
		/// <summary>
		/// Used to retrieve a reference of an interface offered by the upstream connected node.
		/// </summary>
		/// <param name="UpstreamInterface">The retrieved interface.</param>
		void GetUpstreamInterface([MarshalAs(UnmanagedType.IUnknown)] out object UpstreamInterface);
		void SetConnectionHandler(IConnectionHandler handler, [MarshalAs(UnmanagedType.IUnknown)] object sink);
	}
	
	/// <summary>
	/// Interface to an OutputPin of the generic node type
	/// </summary>
	[Guid("5D4F7524-CC1B-44FA-881F-A88D343D7A21"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface INodeOut: IPluginOut
	{
		/// <summary>
		/// Used to set the interface this
		/// </summary>
		/// <param name="TheInterface"></param>
		[Obsolete("Replaced by SetInterface(object TheInterface).")]
		void SetInterface(INodeIOBase TheInterface);
		/// <summary>
		/// Used to set the SubType of a node pin, which is a more detailed specification of the node type via a set of Guids that identifiy the interfaces offered on this pin.
		/// The SubType is used by the GUI to guide the user to make only links between pins that understand the same interfaces.
		/// Should only be called once immediately after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost(IPluginHost)">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="Guids">An array of Guids (typically only one) that specifies the interfaces that this output accepts.</param>
		/// <param name="FriendlyName">A user readable name specifying the type of the node connection.</param>
		void SetSubType(Guid[] Guids, string FriendlyName);
		/// <summary>
		/// Used to mark this pin as being changed compared to the last frame. 
		/// </summary>
		void MarkPinAsChanged();
		/// <summary>
		/// Used to set the interface this
		/// </summary>
		/// <param name="TheInterface"></param>
		void SetInterface([MarshalAs(UnmanagedType.IUnknown)] object TheInterface);
		void SetConnectionHandler(IConnectionHandler handler, [MarshalAs(UnmanagedType.IUnknown)] object source);
	}
	
	/// <summary>
	/// Interface to an InputPin of type Transform.
	/// </summary>
	[Guid("605FD0B2-AD68-40B4-92E5-819599544CF2"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	unsafe public interface ITransformIn: IPluginIn
	{
		/// <summary>
		/// Used to retrieve a Matrix from the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to retrieve the Matrix from.</param>
		/// <param name="Value">The retrieved Matrix.</param>
		void GetMatrix(int Index, out Matrix4x4 Value);
		/// <summary>
		/// Used to retrieve a Pointer to the Values of the pin, which can be used to retrive large Spreads of Values more efficiently.
		/// Attention: Don't use this Pointer to write Values to the pin!
		/// </summary>
		/// <param name="SliceCount">The pins current SliceCount, specifying the number of values accessible via the Pointer.</param>
		/// <param name="Value">A Pointer to the pins first Value.</param>
		void GetMatrixPointer(out int SliceCount, out float* Value);
		void GetMatrixPointer(out int* pLength, out float** ppData);
		/// <summary>
		/// Used to retrieve a World Matrix from the pin at the specified slice. 
		/// You should call this method only from within your Render method when supporting the IPluginDXLayer interface.
		/// </summary>
		/// <param name="Index">The index of the slice to retrieve the Matrix from.</param>
		/// <param name="Value">The retrieved Matrix.</param>
		void GetRenderWorldMatrix(int Index, out Matrix4x4 Value);
		/// <summary>
		/// Used to initialize rendering by letting vvvv know which transform pin controls spaces. 
		/// This sets view and projection matrices.
		/// </summary>
		void SetRenderSpace();
		/// <summary>
		/// Used to retrieve a World Matrix from the pin at the specified slice. 
		/// You should call this method only from within your Render method when supporting the IPluginDXLayer interface.
		/// </summary>
		/// <param name="Index">The index of the slice to retrieve the Matrix from.</param>
		/// <param name="Value">The retrieved Matrix.</param>
		void GetRenderWorldMatrix(int Index, [Out, MarshalAs(UnmanagedType.Struct)] out Matrix Value);
	}
	
	/// <summary>
	/// Interface to an OutputPin of type Transform.
	/// </summary>
	[Guid("AA8D6410-36E5-4EA2-AF70-66CD6321FF36"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	unsafe public interface ITransformOut: IPluginOut
	{
		/// <summary>
		/// Used to write a Matrix to the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to write the Matrix to.</param>
		/// <param name="Value">The Matrix to write.</param>
		void SetMatrix(int Index, Matrix4x4 Value);
		/// <summary>
		/// Used to retrieve a Pointer to the Values of the pin, which can be used to write large number of values more efficiently.
		/// Note though, that when writing Values to the Pointer the pins dimensions and overall SliceCount have to be taken care of manually.
		/// </summary>
		/// <param name="Value">A Pointer to the pins first Value.</param>
		void GetMatrixPointer(out float* Value);
		void GetMatrixPointer(out float** ppDst);
	}
	#endregion node pins	
	
	#region DXPins
	/// <summary>
	/// Interface to an OutputPin of type DirectX Mesh.
	/// </summary>
	[Guid("4D7E1619-0342-48EE-8AD0-13245226FD99"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IDXMeshOut: IPluginOut
	{
		/// <summary>
		/// Used to mark the mesh as being changed compared to the last frame. 
		/// </summary>
		void MarkPinAsChanged();
	}
	
	[Guid("3E1B832D-FF75-4BC3-A894-47BC84A6199E"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IDXTextureOut: IPluginOut
	{
		/// <summary>
		/// Used to mark the texture as being changed compared to the last frame. 
		/// </summary>
		void MarkPinAsChanged();
	}
	
	/// <summary>
	/// Interface to an OutputPin of type DirectX Layer.
	/// </summary>
	[Guid("513190D5-68C5-4623-9BDA-D5C2B8D50172"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IDXLayerIO: IPluginOut
	{
		
	}
	
	/// <summary>
	/// Base interface to all InputPins of type DirectX State.
	/// </summary>
	[Guid("9A09094D-4627-4CA4-A65D-D9FC2295FAB8"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IDXStateIn: IPluginIn
	{
		/// <summary>
		/// Used to set States connected to this input slicewise during the RenderLoop.
		/// </summary>
		/// <param name="Index">The Index of the currently rendered slice</param>
		void SetSliceStates(int Index);
	}
	
	/// <summary>
	/// Interface to an InputPin of type DirectX RenderState.
	/// </summary>
	[Guid("18DC7340-1AF3-46A5-9FF7-3348644DAB05"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IDXRenderStateIn: IDXStateIn
	{
		/// <summary>
		/// Used to set RenderStates from within <see cref="VVVV.PluginInterfaces.V1.IPluginDXLayer.SetStates()">IPluginDXLayer.SetStates</see>.
		/// </summary>
		/// <param name="State">The RenderState</param>
		/// <param name="Value">The RenderStates value</param>
		void SetRenderState([MarshalAs(UnmanagedType.I4)] RenderState State, int Value);
	}
	
	/// <summary>
	/// Interface to an InputPin of type DirectX SamplerState.
	/// </summary>
	[Guid("49D69B50-6498-4B19-957E-828D86CD9E45"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IDXSamplerStateIn: IDXStateIn
	{
		/// <summary>
		///  Used to set SamplerStates from within <see cref="VVVV.PluginInterfaces.V1.IPluginDXLayer.SetStates()">IPluginDXLayer.SetStates</see>.
		/// </summary>
		/// <param name="Sampler">The sampler index to apply the SamplerState to</param>
		/// <param name="State">The SamplerState</param>
		/// <param name="Value">The SamplerStates value</param>
		void SetSamplerState(int Sampler, [MarshalAs(UnmanagedType.I4)] SamplerState State, int Value);
		/// <summary>
		/// Used to set TextureStageStates from within <see cref="VVVV.PluginInterfaces.V1.IPluginDXLayer.SetStates()">IPluginDXLayer.SetStates</see>.
		/// </summary>
		/// <param name="Sampler"></param>
		/// <param name="State"></param>
		/// <param name="Value"></param>
		void SetTextureStageState(int Sampler, [MarshalAs(UnmanagedType.I4)] TextureStage State, int Value);
	}
	#endregion DXPins
}
