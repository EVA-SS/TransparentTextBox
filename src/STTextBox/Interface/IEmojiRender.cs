namespace TSkin.ST
{
    public interface IEmojiRender
    {
        bool IsEmoji(string strChar);
        void DrawEmoji(ISTTextBoxRender dt, string strChar, int nX, int nY, int nWidth, bool bSelected);
    }
}
