namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Barometer.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Barometer']/Docs" />
	public static partial class Barometer
	{
		internal static bool IsSupported =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		internal static void PlatformStart(SensorSpeed sensorSpeed) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		internal static void PlatformStop() =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
