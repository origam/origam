using System.Drawing;
using static Origam.NewProjectEnums;

namespace Origam.Git
{
    public class WebGitData
    {
        public WebGitData(Image avatar, string name, string link, string readme, TypeTemplate typeTemplate)
        {
            this.avatar = avatar;
            this.RepositoryName = name;
            this.RepositoryLink = link;
            this.Readme = readme;
            this.TypeTemplate = typeTemplate;
        }
        public WebGitData(Image avatar, string name, string link, string readme)
        {
            this.avatar = avatar;
            this.RepositoryName = name;
            this.RepositoryLink = link;
            this.Readme = readme;
        }

        public Image avatar { get;  }
        public string RepositoryName { get; }
        public string RepositoryLink { get;  }
        public string Readme { get;  }
        public TypeTemplate TypeTemplate { get; set; }

    }
}
