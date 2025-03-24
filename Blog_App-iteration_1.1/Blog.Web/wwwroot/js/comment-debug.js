// Comment debugging helper
window.debugCommentSystem = function() {
    // Check SignalR connection status
    function checkSignalR() {
        console.log("=== SignalR Status ===");
        const commentHub = document.querySelector('[src*="signalr.min.js"]');
        console.log("SignalR script loaded:", !!commentHub);
        console.log("Connection object exists:", typeof connection !== 'undefined');
        if (typeof connection !== 'undefined') {
            console.log("Connection state:", connection.state);
            console.log("ConnectionId:", connection.connectionId);
        }
    }
    
    // Check if we can find required elements
    function checkDomElements() {
        console.log("=== DOM Elements ===");
        console.log("Form exists:", !!document.getElementById('commentForm'));
        console.log("Content textarea:", !!document.getElementById('commentContent'));
        console.log("Article ID input:", !!document.getElementById('articleId'));
        console.log("Antiforgery token:", !!document.querySelector('input[name="__RequestVerificationToken"]'));
    }
    
    // Test manual comment submission
    function testCommentSubmission() {
        console.log("=== Testing Manual Comment Submission ===");
        const articleIdElem = document.getElementById('articleId');
        if (!articleIdElem) {
            console.log("Cannot find article ID element");
            return;
        }
        
        const articleId = articleIdElem.value;
        console.log("Article ID:", articleId);
        
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
        if (!token) {
            console.log("Cannot find antiforgery token");
            return;
        }
        
        const comment = {
            content: "Test comment from debug helper " + new Date().toISOString(),
            articleId: parseInt(articleId)
        };
        
        console.log("Submitting test comment:", comment);
        
        fetch('/Comments', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            },
            body: JSON.stringify(comment)
        })
        .then(response => {
            console.log("Response status:", response.status);
            return response.json();
        })
        .then(data => {
            console.log("Response data:", data);
        })
        .catch(error => {
            console.error("Error submitting comment:", error);
        });
    }
    
    // Test comment deletion
    function testDeleteComment(commentId) {
        console.log(`=== Testing Delete Comment ${commentId} ===`);
        
        if (!commentId) {
            console.error("No comment ID provided");
            return;
        }
        
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
        if (!token) {
            console.log("Cannot find antiforgery token");
            return;
        }
        
        console.log(`Deleting comment ID: ${commentId}`);
        
        fetch(`/Comments/${commentId}`, {
            method: 'DELETE',
            headers: {
                'RequestVerificationToken': token
            }
        })
        .then(response => {
            console.log("Response status:", response.status);
            return response.json();
        })
        .then(data => {
            console.log("Response data:", data);
        })
        .catch(error => {
            console.error("Error deleting comment:", error);
        });
    }
    
    // Run all checks
    checkSignalR();
    checkDomElements();
    console.log("To test manual comment submission, call: debugCommentSystem.testSubmit()");
    console.log("To test comment deletion, call: debugCommentSystem.testDelete(commentId)");
    
    // Return public methods
    return {
        testSubmit: testCommentSubmission,
        testDelete: testDeleteComment,
        checkSignalR: checkSignalR,
        checkDom: checkDomElements
    };
}();

console.log("Comment debugging helper loaded. Call debugCommentSystem() to run diagnostics."); 