using SmartInspect.Vision;

VisionService visionService = new VisionService();

string templatePath =
    @"D:\MachineVision\05_projects\SmartInspect\02_Source\VisionImages\Template\template_part.png";

string correctScenePath =
    @"D:\MachineVision\05_projects\SmartInspect\02_Source\VisionImages\Template\template_scene.png";

string wrongScenePath =
    @"D:\MachineVision\05_projects\SmartInspect\02_Source\VisionImages\Basic\07_multiple_parts.png";

const double minimumScore = 0.80;

Console.WriteLine("SmartInspect Template Score Test");
Console.WriteLine();

Console.WriteLine("========== 测试 1：正确场景 ==========");

TemplateMatchResult correctResult =
    visionService.MatchTemplate(
        correctScenePath,
        templatePath,
        minimumScore);

PrintResult(correctResult);

Console.WriteLine();
Console.WriteLine("========== 测试 2：错误场景 ==========");

TemplateMatchResult wrongResult =
    visionService.MatchTemplate(
        wrongScenePath,
        templatePath,
        minimumScore);

PrintResult(wrongResult);

Console.WriteLine();
Console.WriteLine("当前匹配阈值：" + minimumScore.ToString("F2"));

Console.WriteLine();
Console.WriteLine("按 Enter 退出");

Console.ReadLine();

static void PrintResult(TemplateMatchResult result)
{
    Console.WriteLine("Success：" + result.Success);
    Console.WriteLine("Message：" + result.Message);
    Console.WriteLine("匹配分数：" + result.Score.ToString("F4"));

    Console.WriteLine(
        "最高分位置：(" +
        result.MatchX +
        ", " +
        result.MatchY +
        ")");
}