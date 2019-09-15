
using bub.Visual.Detail;

namespace bub.Visual.Interfaces
{
    public interface IBubbler
    {
        bool IsSelected(int x, int y);
        Selection Selection { get; }

    }
}
