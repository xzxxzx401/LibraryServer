using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LibrarySystemBackEnd {
	/// <summary>
	/// 协议处理类，辅助
	/// </summary>
	class ProtocolHandler {
		/// <summary>
		/// 缓存不完整的部分字符串
		/// </summary>
		private string partialProtocal;

		public ProtocolHandler() {
			partialProtocal = "";
		}

		/// <summary>
		/// 获取协议
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public string[] GetProtocol(string input) {
			return GetProtocol(input, null);
		}

		/// <summary>
		/// 获取协议
		/// </summary>
		/// <param name="input"></param>
		/// <param name="outputList"></param>
		/// <returns></returns>
		public string[] GetProtocol(string input, List<string> outputList) {
			if (outputList == null)
				outputList = new List<string>();
			if (String.IsNullOrEmpty(input))
				return outputList.ToArray();
			if (!String.IsNullOrEmpty(partialProtocal))
				input = partialProtocal + input;
			string pattern = "(^<protocol>.*?</protocol>)";

			if (Regex.IsMatch(input, pattern, RegexOptions.Singleline)) {
				string match = Regex.Match(input, pattern, RegexOptions.Singleline).Groups[0].Value;
				outputList.Add(match);
				partialProtocal = "";

				input = input.Substring(match.Length);

				GetProtocol(input, outputList);
			} else {
				partialProtocal = input;
			}
			return outputList.ToArray();
		}
	}
}
