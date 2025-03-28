// API Endpoints
const API_ENDPOINTS = {
    PERMISSIONS: {
        PROCESS: '/Permissions/Process'
    },
    ARTICLE_VOTES: {
        VOTE: '/api/articlevotes/vote'
    },
    COMMENTS: {
        GET_ARTICLE_COMMENTS: '/Comments/Article/',
        CREATE: '/Comments',
        UPDATE: '/Comments/',  // Append comment ID
        DELETE: '/Comments/',  // Append comment ID
        REPORT: '/Comments/Report',
        BLOCK: '/Comments/Block'
    }
};

// Export for use in other files
if (typeof module !== 'undefined' && module.exports) {
    module.exports = API_ENDPOINTS;
} 