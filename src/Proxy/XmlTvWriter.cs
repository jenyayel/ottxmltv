using Proxy.Ott;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;

namespace Proxy
{
    public class XmlTvWriter : IDisposable
    {
        private const string ICONS_HOST = "http://ott.watch/images/";
        private XmlWriter _writer;

        public XmlTvWriter(Stream output)
        {
            if (output == null) throw new ArgumentNullException(nameof(output));
            _writer = XmlWriter.Create(output, new XmlWriterSettings { Async = true });

            _writer.WriteDocType("tv", "xmltv.dtd", "SYSTEM", null);

            writeStartElement("tv");
            writeAttribute("generator-info-name", "OttXmltv");
        }

        public async Task Write(Channel[] channels)
        {

            foreach (var channel in channels)
            {
                writeStartElement("channel");
                writeAttribute("id", channel.ChannelId);

                writeStartElement("display-name");
                writeAttribute("lang", "ru");
                _writer.WriteValue(channel.ChannelName);
                _writer.WriteEndElement(); //display-name

                writeStartElement("icon");
                writeAttribute("src", ICONS_HOST + channel.ChannelImage);
                _writer.WriteEndElement(); // icon

                _writer.WriteEndElement(); //channel
            }

            await _writer.FlushAsync();
        }

        public async Task Write(IEnumerable<Programme> programmes)
        {
            foreach (var programme in programmes)
            {
                writeStartElement("programme");
                writeAttribute("channel", programme.Channel.ChannelId);
                writeAttribute("start", programme.StartTime);
                writeAttribute("stop", programme.EndTime);

                writeStartElement("title");
                writeAttribute("lang", "ru");
                _writer.WriteValue(programme.Name);
                _writer.WriteEndElement(); //title

                if (!String.IsNullOrEmpty(programme.Description))
                {
                    writeStartElement("desc");
                    writeAttribute("lang", "ru");
                    _writer.WriteValue(programme.Description);
                    _writer.WriteEndElement(); //desc
                }

                _writer.WriteEndElement(); //programme
                await _writer.FlushAsync();
            }
        }

        public void Dispose()
        {
            _writer.WriteEndElement(); //tv
            _writer.Flush();
            _writer.Dispose();
        }

        private void writeStartElement(string name) => _writer.WriteStartElement(name, null);

        private void writeAttribute(string name, object value) => _writer.WriteAttributeString(name, null, value is string ? (string)value : value.ToString());
    }
}
