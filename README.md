# CodedChatbot
Twitch chatbot centred around taking song requests

### Config
CodedChatbot Project should include a config.json file at it's root as below:

```
{
    "TwitchHost": "irc.chat.twitch.tv",
    "TwitchPort": 6667,
    "ChatbotNick": "", // Twitch username
    "ChatbotPass": "", // Twitch oauth token
    "ChatbotAccessToken": "", // Twitch access token
    "ObsPlaylistPath": "", // Path for playlist txt file when running locally
    "StreamerChannel": "", // Twitch channel the bot should join
    "DiscordLink": "", // Link to Discord server
    "TwitterLink": "", // Link to Twitter handle
    "StreamLabsClientId": "", // Streamlabs login client id (temp)
    "StreamLabsClientSecret": "", // StreamLabs login client secret (temp)
    "StreamLabsCode": "", // StreamLabs access code (temp)
    "LocalDbLocation": "", // Local path for chatbot database (sqlite)
    "WebPlaylistUrl": "" // URL of playlist
}
```