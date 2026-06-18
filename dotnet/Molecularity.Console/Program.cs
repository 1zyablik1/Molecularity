using Molecularity.Console;
using Molecularity.Console.ConsoleInputProvider;
using Molecularity.Console.Rendering;
using Molecularity.Core.Data;

var loop = new ConsoleGameLoop(
    new JsonLevelRepository(System.IO.Path.Combine(AppContext.BaseDirectory, "levels")),
    new ConsoleRenderer(),
    new ConsoleInputProvider()
);
loop.Run();
