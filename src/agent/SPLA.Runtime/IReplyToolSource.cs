namespace SPLA.Runtime;

/// <summary>
/// What <see cref="ChatToolHost"/> needs from the chat that owns it to mix in virtual
/// <c>reply_&lt;role&gt;[_&lt;n&gt;]</c> tools (PLAN_20260902 wave 5; PLAN_20260906 wave 0) — the live
/// set of open correspondences, and a way to deliver across one. <see cref="ChatRuntime"/> is the only
/// production implementer; the seam exists so a test can supply a small fake instead of standing up a
/// whole project (real <see cref="ChatRegistry"/>, disk-backed chats) just to prove
/// <c>ChatToolHost</c>'s own mixing and gating logic — the two members below are exactly, and only,
/// what that logic touches.
/// </summary>
public interface IReplyToolSource
{
    IReadOnlyCollection<Correspondence> Correspondences { get; }

    ChatRuntime.ReplyResult SendReply(string role, int instanceNo, string text);
}
