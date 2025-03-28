// Admin constants for the admin section of the application
window.AdminConstants = {
    Reports: {
        Urls: {
            BlockComment: '/Comments/Block',
            ResolveReport: '/Admin/ResolveReport',
            UnblockComment: '/Comments/Block'
        },
        Confirmation: {
            BlockComment: "Are you sure you want to block this comment?",
            BlockCommentWithReplies: "Are you sure you want to block this comment? This will also block all replies to this comment.",
            ResolveReport: "Are you sure you want to mark this report as resolved?",
            UnblockComment: "Are you sure you want to unblock this comment? This will also unblock all replies to this comment.",
            UnresolveReport: "Are you sure you want to mark this report as unresolved?"
        },
        ErrorMessages: {
            BlockCommentFailed: "Failed to block comment. Please try again later.",
            ResolveReportFailed: "Failed to resolve report. Please try again later.",
            NoReportsFound: "There are no reported comments to review.",
            UnblockCommentFailed: "Failed to unblock comment. Please try again later.",
            NoFilteredResults: "No reports match the current filters."
        },
        Images: {
            DefaultProfile: "/images/default-profile.jpg"
        },
        Status: {
            Resolved: "Resolved",
            Pending: "Pending",
            Blocked: "Blocked",
            Unblocked: "Unblocked"
        },
        ButtonLabels: {
            BlockComment: "Block Comment",
            UnblockComment: "Unblock Comment",
            Resolve: "Resolve",
            Unresolve: "Unresolve",
            View: "View"
        },
        FilterLabels: {
            All: "All Reports",
            Resolved: "Resolved Reports",
            Pending: "Pending Reports",
            AllComments: "All Comments",
            BlockedComments: "Blocked Comments",
            UnblockedComments: "Unblocked Comments"
        }
    }
}; 