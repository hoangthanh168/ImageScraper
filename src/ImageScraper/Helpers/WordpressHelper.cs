using DocumentFormat.OpenXml.Office2010.Excel;
using ImageScraper.Extensions;
using ImageScraper.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WordPressPCL;
using WordPressPCL.Models;

namespace ImageScraper.Helpers
{
    public class WordpressHelper
    {
        public static WordPressClient _client;
        public static WordpressAccount _wordpressAccount;
        public static List<dynamic> users = new List<dynamic>();
        public static List<Category> categories;
        public static List<Tag> tags;
        public static WordpressHelper Instance { get; } = new WordpressHelper();

        public async Task<bool> LoginAsync(WordpressAccount wordpressAccount)
        {
            _wordpressAccount = wordpressAccount;
            try
            {
                _client = new WordPressClient(wordpressAccount.Url + "wp-json/");
                _client.Auth.UseBearerAuth(JWTPlugin.JWTAuthByEnriqueChavez);
                await _client.Auth.RequestJWTokenAsync(wordpressAccount.Username, wordpressAccount.Password).ConfigureAwait(false);
                var isValidToken = await _client.Auth.IsValidJWTokenAsync();
                return isValidToken;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Lỗi đăng nhập", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<IList<string>> GetUsersAsync()
        {
            var userList = new List<string>();
            var response = await _client.CustomRequest.GetAsync<List<dynamic>>("custom-route/v1/users/role?role=author");
            var userIdList = new List<int>();
            foreach (var userDeserialized in response)
            {
                string userName = userDeserialized.data.display_name.ToString();
                int userId = userDeserialized.data.ID;
                userList.Add(userName);
                userIdList.Add(userId);
            }
            foreach (var userId in userIdList)
            {
                dynamic user = await _client.CustomRequest.GetAsync<dynamic>("custom-route/v1/users/id?id=" + userId);
                users.Add(user);
            }
            return userList;
        }

        public async Task<string> GetSlugAsync(string title)
        {
            var res = await _client.CustomRequest.GetAsync<string>($"custom-route/v1/slug/title?title={title}");
            return res;
        }

        public async Task<IList<string>> GetCategoriesAsync()
        {
            List<string> categoryNames = new List<string>();
            var respone = await _client.Categories.GetAllAsync();
            categories = respone.ToList();
            categoryNames.AddRange(respone.Select(x => x.Name).ToList());
            return categoryNames;
        }

        public async Task<IList<string>> GetTagsAsync()
        {
            List<string> tagNames = new List<string>();
            var respone = await _client.Tags.GetAllAsync();
            tags = respone.ToList();
            tagNames.AddRange(respone.Select(x => x.Name).ToList());
            return tagNames;
        }
        public async Task<int?> CheckMediaExistsAsync(string filename)
        {
            try
            {
                var response = await _client.CustomRequest.GetAsync<dynamic>($"custom-route/v1/media/filename?filename={filename}");
               
                if (response["id"] != null)
                {
                    return response["id"];
                }
            }
            catch (Exception)
            {
            }
            
            return null;
        }
        public async Task<MediaItem> CreateMedia(string path)
        {
            string fileName = Path.GetFileName(path);

            // Kiểm tra xem media đã tồn tại chưa
            int? id = await CheckMediaExistsAsync(fileName);

            if (id == null)
            {
                using (Stream s = File.OpenRead(path))
                {
                    var createdMedia = (await _client.Media.CreateAsync(s, fileName));
                    return createdMedia;
                }
            }
            else
            {
                await _client.Media.DeleteAsync((int)id);
                using (Stream s = File.OpenRead(path))
                {
                    var createdMedia = (await _client.Media.CreateAsync(s, fileName));
                    return createdMedia;
                }
            }
        }

        public async Task<int> GetCategoryIDByNameAsync(string name)
        {
            var response = await _client.CustomRequest.GetAsync<dynamic>($"custom-route/v1/category/name?name={name}");
            int id = response.id;
            return id;
        }

        public async Task<int> GetTagIDByNameAsync(string name)
        {
            var response = await _client.CustomRequest.GetAsync<dynamic>($"custom-route/v1/tag/name?name={name}");
            int id = response.id;
            return id;
        }

        public async Task<int> GetPostCountByTagAsync(string name)
        {
            var response = await _client.CustomRequest.GetAsync<dynamic>($"custom-route/v1/tag/posts-count?name={name}");
            int posts_count = response.posts_count;
            return posts_count;
        }

        public async Task<bool> CheckTagExitsAsync(string name)
        {
            var response = await _client.CustomRequest.GetAsync<dynamic>($"custom-route/v1/tag/exists?name={name}");
            bool exists = Convert.ToBoolean(response.exists);
            return exists;
        }

        public async Task<int> CreateCategory(string title, string description="")
        {
            try
            {
                var category = new Category()
                {
                    Name = title,
                    Description = description
                };
                int id = (await _client.Categories.CreateAsync(category)).Id;
                return id;
            }
            catch (Exception)
            {
                int id = await GetCategoryIDByNameAsync(title);
                return id;
            }
        }

        public async Task<int> CreateTag(string title, string description="")
        {
            try
            {
                var tag = new Tag()
                {
                    Name = title,
                    Description = description
                };
                int id = (await _client.Tags.CreateAsync(tag)).Id;
                return id;
            }
            catch (Exception)
            {
                int id = await GetTagIDByNameAsync(title);
                return id;

            }
        }

        public async Task<bool> UpdateTag(int id, string title, string description)
        {
            try
            {
                var tag = new Tag()
                {
                    Id = id,
                    Name = title,
                    Description = description
                };
                await _client.Tags.UpdateAsync(tag);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task PostAsync(Title title, Content content, string featuredImageUrl, int authorId, List<int> tagId, List<int> categoriesId, Status status,string header)
        {
            InpostHeadScript inpostHeadScript = new InpostHeadScript();
            inpostHeadScript.SynthHeaderScript = header;
            CustomPost post = new CustomPost()
            {
                Content = content,
                Title = title,
                Author = authorId,
                Tags = tagId,
                Categories = categoriesId,
                Status = status,
                FifuImageUrl = featuredImageUrl,
                InpostHeadScript = inpostHeadScript
            };
            await _client.Posts.CreateAsync(post);
        }
    }
}