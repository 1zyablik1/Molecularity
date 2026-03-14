using Molecularity.Console;
using Molecularity.Console.ConsoleInputProvider;
using Molecularity.Console.Rendering;
using Molecularity.Core.Data;

var loop = new ConsoleGameLoop(
    new HardcodedLevelRepository(),
    new ConsoleRenderer(),
    new ConsoleInputProvider()
);
loop.Run();
