using CommandLine;
using CommandLine.Text;

namespace VeeamTest
{
    internal class Options
    {
        [Option('c', "compress", Required = false, HelpText = "Define commpress proccess.")]
        public bool IsCompress
        { get; set; }

        [Option('d', "decompress", Required = false, HelpText = "Define decommpress proccess.")]
        public bool IsDecompress
        { get; set; }

        [Option('i', "input", Required = true, HelpText = "Input file.")]
        public string InputFileName
        { get; set; }

        [Option('o', "output", Required = true, HelpText = "Output file.")]
        public string OutputFileName
        { get; set; }

        [Option('s', "size", HelpText = "Size of block in bytes.", DefaultValue = 1048576)]
        public int Blocksize
        { get; set; }

        [Option("hash", Required = false, DefaultValue = Hasher.HashTypes.MD5, HelpText = "Type of hash type. [MD5|SHA256]")]
        public Hasher.HashTypes HashType
        { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this, current => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}
