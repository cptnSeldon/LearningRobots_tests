using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Drawing;
using System.IO;

namespace P3_LearningRobots.Data
{
    public static class JSONHandler
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="rectangles"></param>
        /// <param name="history"></param>
        public static void Save(string filePath, RectangleManager rectangles, History history)
        {
            JObject allTheInfo = new JObject();
            JArray rectangleInfo = new JArray();
            JArray historyCoordinates = new JArray();

            List<Rectangle> r = rectangles.GetAll();

            if (r.Count != 0)
            {
                //retrieving all the rectangles coordinates
                for (int i = 0; i < r.Count; i++)
                {
                    float x, y, width, height;
                    x = rectangles.getPosition(r[i])[0];
                    y = rectangles.getPosition(r[i])[1];
                    width = rectangles.getPosition(r[i])[2];
                    height = rectangles.getPosition(r[i])[3];

                    JObject rectangleObject = new JObject
                    {
                        { "index", i },
                        { "x", x },
                        { "y", y },
                        { "width", width },
                        { "height", height }
                    };

                    rectangleInfo.Add(rectangleObject);
                }

                allTheInfo.Add("Rectangles", rectangleInfo);
            }

            if(history.GetAll() != null)
            {
                IList<HandMetadata> list = history.GetAll().Where((x, i) => i % 5 == 0).ToList();

                foreach (HandMetadata h in list)
                {
                    JObject data = new JObject()
                    {
                        {"left x:", h.LeftHandPosition[0] },
                        {"left y:", h.LeftHandPosition[1] },
                        {"right x:", h.RightHandPosition[0] },
                        {"right y:", h.RightHandPosition[1] },
                    };

                    historyCoordinates.Add(data);

                }
                allTheInfo.Add("Coordinates", historyCoordinates);
            }

            if(allTheInfo != null)
                File.WriteAllText(filePath, allTheInfo.ToString());
        }
    }
}
