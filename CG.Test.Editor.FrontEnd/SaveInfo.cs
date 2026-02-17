using CG.Test.Editor.FrontEnd.Views.Dialogs;
using System.IO;
using System.Text.Json;

namespace CG.Test.Editor.FrontEnd
{
    public class SaveInfo
    {
		private const string SAVE_FILE_NAME = "save.json";

		public required IEnumerable<string> RecentSchemas { get; set; }

        public required string LastColorSchemeName { get; set; }

		public required string LastStyleName { get; set; }

		public static async Task<SaveInfo> LoadAsync()
		{
			if (!File.Exists(SAVE_FILE_NAME))
			{
				await using var creationStream = File.Create(SAVE_FILE_NAME);
				await JsonSerializer.SerializeAsync(creationStream, new SaveInfo()
				{
					RecentSchemas = [],
					LastColorSchemeName = string.Empty,
					LastStyleName = string.Empty,
				});
			}

			await using var stream = File.OpenRead(SAVE_FILE_NAME);
			return (await JsonSerializer.DeserializeAsync<SaveInfo>(stream))!;
		}

		public async Task SaveAsync()
		{
			await using var stream = File.Create(SAVE_FILE_NAME);
			await JsonSerializer.SerializeAsync(stream, this);
		}
	}
}
