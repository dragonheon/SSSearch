using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSearchScreen
{
    internal class DeleteImage
    {
        //Images 폴더 내의 모든 이미지 삭제
        public void DeleteAllImages()
        {
            string[] files = System.IO.Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Images");
            foreach (string file in files)
            {
                System.IO.File.Delete(file);
            }
        }

    }
}
