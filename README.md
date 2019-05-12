# C# Raspberry Pi GPIO

This is a small project to use Raspberry Pi Gpio in C# Application.

## Getting Started

Run *make.bat* to compile *src/GPIO.cs* to dll File.
OR Include this *GPIO.cs* file in your Project.
The GPIO.cs is in namespace *RaspberryPi*.

### Prerequisites

* Raspbian
* Mono
* Microsoft .NET Framework 4.0 or higher

## Running the tests

Tests only run in Raspbian

## Built With

* C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe
* mono

## Example

### Output
```
WriteGPIO gpio = new WriteGPIO ( Pin.gpio17 );
gpio.Write ( true );
Thread.Sleep ( 1000 );
gpio.Write ( false );
```

### Input
```
ReadGPIO gpio = new ReadGPIO ( Pin.gpio18 );
if ( gpio.Read (  ) )
{
    Console.WriteLine ( "An" );
}
else
{
    Console.WriteLine ( "Aus" );
}
```

## Authors

* **Robin D'Andrea** - *Robinterra* - [Robinterra](https://github.com/Robinterra)