using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace RWAnalog.Classes
{
    public struct GeneralConfiguration
    {
        public string PluginsPath { get; set; }
        public Guid SelectedDevice { get; set; }
    }

    public static class SaveLoadManager
    {
        public static void Save(string fileName, List<Train> trains, GeneralConfiguration configuration)
        {
            XmlWriter xmlWriter = XmlWriter.Create(fileName);

            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("RWAnalogConfig");

            xmlWriter.WriteStartElement("PluginsPath");
            xmlWriter.WriteString(configuration.PluginsPath);
            xmlWriter.WriteEndElement();
            xmlWriter.WriteStartElement("SelectedDevice");
            xmlWriter.WriteString(configuration.SelectedDevice.ToString());
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("Trains");
            foreach (Train train in trains)
            {
                xmlWriter.WriteStartElement("Train");
                xmlWriter.WriteAttributeString("Engine", train.ToSingleString());
                xmlWriter.WriteAttributeString("RawControls", train.ToSingleControlsString());

                xmlWriter.WriteStartElement("Controls");
                foreach (TrainControl trainControl in train.Controls)
                {
                    if (trainControl.AssociatedAxis == null)
                        continue;

                    xmlWriter.WriteStartElement("Control");
                    xmlWriter.WriteAttributeString("Name", trainControl.Name);
                    xmlWriter.WriteAttributeString("ControllerId", trainControl.ControllerId.ToString());
                    xmlWriter.WriteAttributeString("AxisOffset", trainControl.AssociatedAxis.AxisOffset.ToString());

                    xmlWriter.WriteStartElement("Graph");
                    foreach (GraphPoint point in trainControl.OverrideInputGraph.Points)
                    {
                        xmlWriter.WriteStartElement("Point");
                        xmlWriter.WriteAttributeString("x", point.X.ToString());
                        xmlWriter.WriteAttributeString("y", point.Y.ToString());
                        xmlWriter.WriteEndElement();
                    }
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteEndElement();
                }
                xmlWriter.WriteEndElement();

                xmlWriter.WriteEndElement();
            }

            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndDocument();

            xmlWriter.Close();
            xmlWriter.Dispose();
        }

        public static GeneralConfiguration? LoadConfiguration(string fileName)
        {
            if (!File.Exists(fileName))
                return null;

            GeneralConfiguration configuration = new GeneralConfiguration();

            XmlReader xmlReader = XmlReader.Create(fileName);

            string name = "";
            while (xmlReader.Read())
            {
                switch (xmlReader.NodeType)
                {
                    case XmlNodeType.Element:
                        name = xmlReader.Name;
                        break;
                    case XmlNodeType.Text:
                        switch (name)
                        {
                            case "PluginsPath":
                                configuration.PluginsPath = xmlReader.Value;
                                break;
                            case "SelectedDevice":
                                configuration.SelectedDevice = Guid.Parse(xmlReader.Value);
                                break;
                            default:
                                break;
                        }
                        break;
                    default:
                        break;
                }
            }

            xmlReader.Close();
            xmlReader.Dispose();

            if (configuration.SelectedDevice == null)
                return null;

            return configuration;
        }

        public static List<Train> LoadTrains(string fileName)
        {
            if (!File.Exists(fileName))
                return new List<Train>();

            List<Train> trainList = new List<Train>();
            XmlReader xmlReader = XmlReader.Create(fileName);

            Train currentTrain = null;
            List<TrainControl> controls = new List<TrainControl>();
            TrainControl currentTrainControl = new TrainControl();
            InputGraph currentGraph = new InputGraph(false);
            while (xmlReader.Read())
            {
                switch (xmlReader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (xmlReader.Name)
                        {
                            case "Train":
                                string engine = xmlReader.GetAttribute("Engine");
                                string controlsRaw = xmlReader.GetAttribute("RawControls");
                                currentTrain = new Train(engine, controlsRaw);
                                break;
                            case "Control":
                                string name = xmlReader.GetAttribute("Name");
                                int id = int.Parse(xmlReader.GetAttribute("ControllerId"));
                                int axis = int.Parse(xmlReader.GetAttribute("AxisOffset"));

                                currentTrainControl = new TrainControl(name, id);
                                currentTrainControl.AssociatedAxis = new Axis(axis);
                                currentTrain.Controls[id] = new TrainControl(name, id);
                                break;
                            case "Graph":
                                currentGraph = new InputGraph(false);
                                break;
                            case "Point":
                                float x = float.Parse(xmlReader.GetAttribute("x"));
                                float y = float.Parse(xmlReader.GetAttribute("y"));
                                currentGraph.Points.Add(new GraphPoint(x, y));
                                break;
                            default:
                                break;
                        }
                        break;
                    case XmlNodeType.EndElement:
                        switch (xmlReader.Name)
                        {
                            case "Train":
                                foreach (TrainControl control in controls)
                                {
                                    currentTrain.Controls[control.ControllerId] = control;
                                }
                                trainList.Add(currentTrain);
                                break;
                            case "Control":
                                controls.Add(currentTrainControl);
                                break;
                            case "Graph":
                                currentTrainControl.OverrideInputGraph = currentGraph;
                                break;
                            default:
                                break;
                        }
                        break;
                    default:
                        break;
                }
            }

            xmlReader.Close();
            xmlReader.Dispose();

            return trainList;
        }
    }
}
