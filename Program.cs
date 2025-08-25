using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

internal class Program
{
    private static string GetSHA256(string s)
    {
        FileStream fileStream = new FileStream(s, FileMode.Open);
        byte[] array = SHA256.Create().ComputeHash(fileStream);
        fileStream.Close();
        StringBuilder stringBuilder = new StringBuilder();
        for (int i = 0; i < array.Length; i++)
        {
            stringBuilder.Append(array[i].ToString("x2"));
        }
        return stringBuilder.ToString();
    }
    private static string[] GetOldFolderSHA(string folder)
    {
        string[] files = Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories);
        List<string> list = new List<string>();
        foreach (string file in files)
        {
            string sHA = GetSHA256(file);
            string relativePath = Path.GetRelativePath(folder, file);
            string value = relativePath + "?" + sHA;
            list.Add(value);
            Console.WriteLine(value);
        }
        return [.. list];
    }
    private static void GetDIFFItemsAndCopy(string folder, Dictionary<string, string> dictionary)
    {
        string[] files = Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories);
        List<string> list = new List<string>();
        foreach (string file in files)
        {
            string sHA2 = GetSHA256(file);
            if (!dictionary.ContainsKey(sHA2))
            {
                string rp = Path.GetRelativePath(folder, file);
                list.Add(rp);
                Console.WriteLine("不同的文件：" + rp);
            }
            Console.WriteLine(sHA2 ?? "");
        }
        //copy
        foreach (string item in list)
        {
            try
            {
                string src = Path.Combine(folder, item);
                string dst = Path.Combine(Path.GetFileName(folder) + "_DIFF", item);
                string? folder1 = Path.GetDirectoryName(dst);
                if (folder1 != null) Directory.CreateDirectory(folder1);
                File.Copy(src, dst);
                Console.WriteLine(src + " --> " + dst);
            }
            catch
            {
                continue;
            }
        }
    }
    private static void Main()
    {
        Console.WriteLine("这个程序会计算文件夹中文件的SHA256并形成列表。当文件夹发生变动后，程序再次计算SHA256，并能提取出内容有变化的文件和新增的文件。\r\n注意：两次计算的文件夹路径需一致。\r\n");
        Console.WriteLine("选择模式：C（计算旧文件夹）；U（计算新文件夹并提取DIFF）：");
        string text = Console.ReadLine()!;
        if (text == "C")
        {
            Console.WriteLine("输入旧文件夹路径（可拖拽）：");
            string folder = Console.ReadLine()!.Trim('\"');
            string[] files = GetOldFolderSHA(folder);
            string? fname = Path.GetFileName(folder);
            File.WriteAllLines((fname == null ? folder[0] : fname) + ".sha256list",files);
            Console.WriteLine("完毕！");
            return;
        }
        else if (text == "U")
        {
            Console.WriteLine("输入新文件夹路径（可拖拽）：");
            string newfolder = Console.ReadLine()!.Trim('\"');
            Console.WriteLine("输入sha256list文件路径：");
            string[] array2 = File.ReadAllLines(Console.ReadLine()!.Trim('\"').Replace("\"", ""));
            Dictionary<string, string> dictionary = [];//path,sha256
            foreach (string text5 in array2)
            {
                dictionary.TryAdd(text5.Split('?')[1], text5.Split('?')[0]);
            }
            GetDIFFItemsAndCopy(newfolder,dictionary);
            return;
        }
        else
        {
            //
        }
    }
}
