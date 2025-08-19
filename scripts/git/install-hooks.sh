#!/bin/bash
# Install git hooks for BlockLife project (Linux/Mac version)

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
HOOKS_SOURCE="$SCRIPT_DIR/hooks"
HOOKS_TARGET="$(dirname "$(dirname "$SCRIPT_DIR")")/.git/hooks"

if [ ! -d "$HOOKS_SOURCE" ]; then
    echo "❌ Git hooks source directory not found: $HOOKS_SOURCE"
    exit 1
fi

if [ ! -d "$HOOKS_TARGET" ]; then
    echo "❌ Git hooks target directory not found: $HOOKS_TARGET"
    echo "   Make sure you're running this from the project root"
    exit 1
fi

echo "📦 Installing git hooks..."

# Copy all hook files and make executable
for hook in "$HOOKS_SOURCE"/*; do
    if [ -f "$hook" ]; then
        hook_name=$(basename "$hook")
        cp "$hook" "$HOOKS_TARGET/$hook_name"
        chmod +x "$HOOKS_TARGET/$hook_name"
        echo "   ✅ Installed: $hook_name"
    fi
done

echo ""
echo "🎉 Git hooks installed successfully!"
echo ""
echo "The pre-push hook will now:"
echo "   • Block direct pushes to main"
echo "   • Ensure your branch is rebased on latest main"
echo "   • Prevent merge conflicts and duplicate work"
echo ""
echo "To test the hook, try:"
echo "   git push  (it will check if you're up to date)"
echo ""