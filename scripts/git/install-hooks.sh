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
echo "The hooks will now:"
echo ""
echo "Pre-commit hook:"
echo "   • Run build + tests before each commit"
echo "   • Prevent bad commits from entering history"
echo "   • Save CI resources with fast local validation"
echo ""
echo "Pre-checkout hook:"
echo "   • Validate branch naming (feat/vs-XXX, fix/br-XXX, feat/td-XXX)"
echo "   • Link branches to backlog work items"
echo "   • Guide developers through workflow"
echo ""
echo "Note:"
echo "   GitHub branch protection handles main branch security"
echo "   These hooks focus on quality and workflow guidance"
echo ""
echo "To test the hooks, try:"
echo "   git checkout -b feat/vs-001-test  (validates naming)"
echo "   git commit -m 'test'              (runs build+tests)"
echo ""