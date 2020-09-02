using HttpServer.Http.Request;
using HttpServer.Http.Response;
using System;
using System.Collections.Generic;
using System.Text;

namespace HttpServer.Utils
{
    public class PathURLExtractor
    {
        const int START = 0, LEFT_BRACE = 1, DECIDE_KEY = 2, DECIDE_VALUE = 3, ERROR = -1;
        public static IEnumerable<KeyValuePair<string,string>> ExtractPathVariable(string pattern, string url) {

            if (pattern.Length == 0 || url.Length == 0) {
                yield break;
            }
            
            int pat_ptr = 0, url_ptr = 0, currentState = 0;
            char pat_current = ' ', url_current = ' ';
            StringBuilder key = new StringBuilder();
            StringBuilder value = new StringBuilder();
            while ((pat_current != '\0' || url_current != '\0') && currentState != ERROR)  {
                pat_current = pat_ptr < pattern.Length ? pattern[pat_ptr] : '\0';
                url_current = url_ptr < url.Length ? url[url_ptr] : '\0';
                switch (currentState) {
                    case START:
                        if (pat_current == '{') {
                            currentState = LEFT_BRACE;
                            url_ptr--;
                        }
                        else if (pat_current != url_current) 
                            currentState = ERROR;
                        break;
                    case LEFT_BRACE:
                        if (pat_current == '}') {
                            currentState = DECIDE_VALUE;
                            url_ptr--;
                        }
                        else if(url_current == '/') {
                            currentState = DECIDE_KEY;
                            pat_ptr--;
                            url_ptr--;
                        }
                        else {
                            if (pat_current != '\0')
                                key.Append(pat_current);
                            if (url_current != '\0')
                                value.Append(url_current);
                        }
                        break;
                    case DECIDE_VALUE:
                        if(url_current == '/' || url_current == '\0') {
                            yield return new KeyValuePair<string, string>(
                                    key.ToString(),
                                    value.ToString()
                                );
                            currentState = START;
                            key.Clear();
                            value.Clear();
                        }
                        else {
                            if (url_current != '\0')
                                value.Append(url_current);
                            pat_ptr--;
                        }
                        break;
                    case DECIDE_KEY:
                        if(pat_current == '}') {
                            yield return new KeyValuePair<string, string>(
                                    key.ToString(),
                                    value.ToString()
                                );
                            currentState = START;
                            key.Clear();
                            value.Clear();
                        }
                        else if (pat_current != '\0') {
                            key.Append(pat_current);
                        }
                        url_ptr--;
                        break;
                }
                url_ptr++;
                pat_ptr++;
            }
        }
    }
}
