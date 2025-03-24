// Comments JavaScript functionality
function initializeComments(options) {
    const { currentUserId, canComment, articleId, currentUserProfilePicture } = options;
    
    // Log user information for debugging
    console.log("Comment initialization with currentUserId:", currentUserId);
    
    // SignalR connection
    let connection;
    
    // Store loaded comments
    let commentsCache = [];
    
    // Initialize when document is ready
    $(document).ready(function() {
        // Initialize SignalR connection
        initializeSignalR();
        
        // Load comments
        loadComments();
        
        // Handle comment input focus and input
        $("#commentContent").on("focus", function() {
            $(".comment-actions").slideDown();
        });

        // Handle comment input changes
        $("#commentContent").on("input", function() {
            const content = $(this).val().trim();
            $("#submitCommentBtn").prop("disabled", !content);
            
            // Auto-expand textarea
            this.style.height = "auto";
            this.style.height = (this.scrollHeight) + "px";
        });
        
        // Handle cancel button
        $("#cancelCommentBtn").on("click", function() {
            $("#commentContent").val("").trigger("input").blur();
            $(".comment-actions").slideUp();
        });
        
        // Handle comment form submission
        $("#commentForm").on("submit", function(e) {
            e.preventDefault();
            submitComment();
        });
        
        // Handle edit comment button clicks with extra security check
        $(document).on("click", ".edit-comment-btn", function() {
            const commentId = $(this).data("id");
            const commentAuthor = $(this).closest('.kebab-menu').data('comment-author');
            
            // Extra security check - only allow if current user is the author
            if (!currentUserId || String(currentUserId) !== String(commentAuthor)) {
                console.error('Unauthorized edit attempt');
                alert('You are not authorized to edit this comment');
                return;
            }
            
            openEditModal(commentId);
        });
        
        // Handle delete comment button clicks with extra security check
        $(document).on("click", ".delete-comment-btn", function() {
            const commentId = $(this).data("id");
            const commentAuthor = $(this).closest('.kebab-menu').data('comment-author');
            
            // Extra security check - only allow if current user is the author
            if (!currentUserId || String(currentUserId) !== String(commentAuthor)) {
                console.error('Unauthorized delete attempt');
                alert('You are not authorized to delete this comment');
                return;
            }
            
            deleteComment(commentId);
        });
        
        // Handle reply button clicks
        $(document).on("click", ".reply-comment-btn", function() {
            const commentId = $(this).data("id");
            showReplyForm(commentId);
        });

        // Handle toggle replies
        $(document).on("click", ".toggle-replies", function() {
            const threadId = $(this).data("thread-id");
            toggleReplies(threadId);
        });

        // Handle collapse line click
        $(document).on("click", ".collapse-line", function() {
            const threadId = $(this).data("thread-id");
            toggleReplies(threadId);
        });

        // Handle collapsed indicator click
        $(document).on("click", ".collapsed-indicator", function() {
            const threadId = $(this).closest(".comment-thread").attr("id").replace("thread-", "");
            toggleReplies(threadId);
        });
    });
    
    function initializeSignalR() {
        console.log("Initializing SignalR...");
        connection = new signalR.HubConnectionBuilder()
            .withUrl("/commentHub")
            .withAutomaticReconnect()
            .build();
            
        connection.on("NewComment", function(comment) {
            console.log("SignalR: New comment received", comment);
            // Only append if the comment isn't already in the cache
            if (!commentsCache.some(c => c.id === comment.id)) {
                appendNewComment(comment);
            }
        });
        
        connection.on("UpdateComment", function(comment) {
            console.log("SignalR: Comment update received", comment);
            updateExistingComment(comment);
        });
        
        connection.on("DeleteComment", function(commentId) {
            console.log("SignalR: Delete comment received", commentId);
            removeComment(commentId);
        });
        
        connection.start()
            .then(function() {
                console.log("SignalR Connected");
                connection.invoke("JoinArticleGroup", articleId)
                    .then(function() {
                        console.log("Joined article group:", articleId);
                    })
                    .catch(function(err) {
                        console.error("Error joining article group:", err);
                    });
            })
            .catch(function(err) {
                console.error("SignalR Connection Error:", err);
            });
    }
    
    function loadComments() {
        // Set a timeout to hide the loading spinner after 10 seconds if the AJAX call doesn't complete
        const loadingTimeout = setTimeout(() => {
            $("#commentsLoading").hide();
            $("#commentsList").append(`
                <div class="alert alert-warning">
                    Comments are taking longer than expected to load. Please refresh the page.
                </div>
            `);
        }, 10000);
        
        $.ajax({
            url: `/Comments/Article/${articleId}`,
            method: "GET",
            success: function(comments) {
                clearTimeout(loadingTimeout); // Clear the timeout on success
                commentsCache = comments;
                
                // Update total comment count (including replies)
                const totalComments = comments.length;
                $("#totalCommentCount").text(totalComments);
                
                renderComments(comments);
            },
            error: function(error) {
                clearTimeout(loadingTimeout); // Clear the timeout on error
                console.error("Error loading comments:", error);
                $("#commentsLoading").hide();
                $("#commentsList").append(`
                    <div class="alert alert-danger">
                        Failed to load comments. Please try refreshing the page.
                    </div>
                `);
                // Set comment count to 0 on error
                $("#totalCommentCount").text("0");
            }
        });
    }
    
    function renderComments(comments) {
        $("#commentsLoading").hide();
        
        if (comments.length === 0) {
            $("#noComments").show();
            return;
        }
        
        const parentTemplate = document.getElementById("commentTemplate").innerHTML;
        const replyTemplate = document.getElementById("replyTemplate").innerHTML;
        const commentsByParent = groupCommentsByParent(comments);
        
        // First render all parent comments (those with no parent)
        commentsByParent[null]?.forEach(comment => {
            // Count replies for this comment
            const replies = commentsByParent[comment.id] || [];
            comment.replyCount = replies.length;
            
            const commentHtml = renderCommentHtml(comment, parentTemplate, replies.length > 0);
            $("#commentsList").append(commentHtml);
            
            // Then render any replies to this comment if there are any
            if (replies.length > 0) {
                const repliesContainer = $(`#replies-${comment.id}`);
                replies.forEach(reply => {
                    const replyHtml = renderReplyHtml(reply, replyTemplate);
                    repliesContainer.append(replyHtml);
                });
                
                // By default, collapse all replies
                toggleReplies(comment.id, true);
            } else {
                // Hide the toggle button if there are no replies
                $(`button.toggle-replies[data-thread-id="${comment.id}"]`).hide();
            }
        });
    }
    
    function groupCommentsByParent(comments) {
        return comments.reduce((groups, comment) => {
            const parentId = comment.parentCommentId;
            if (!groups[parentId]) {
                groups[parentId] = [];
            }
            groups[parentId].push(comment);
            return groups;
        }, {});
    }
    
    function renderCommentHtml(comment, template, hasReplies) {
        const createdDate = new Date(comment.createdAt).toLocaleString();
        
        // Only allow editing if the user is logged in (currentUserId is not empty) and owns the comment
        const isLoggedIn = currentUserId && currentUserId.trim() !== "";
        const isOwner = isLoggedIn && String(currentUserId) === String(comment.userId);
        
        // Log information for debugging
        console.log(`Comment ${comment.id} by user ${comment.userId}, current user: ${currentUserId}, isLoggedIn: ${isLoggedIn}, isOwner: ${isOwner}`);
        
        // Completely hide buttons if not the owner
        const displayStyle = isOwner ? 'flex' : 'none';
        
        return template
            .replace(/{id}/g, comment.id)
            .replace(/{userId}/g, comment.userId)
            .replace(/{content}/g, comment.content)
            .replace(/{userName}/g, comment.userName)
            .replace(/{userProfilePicture}/g, comment.userProfilePicture || '/images/default-profile.jpg')
            .replace(/{createdAt}/g, createdDate)
            .replace(/{actionsDisplay}/g, displayStyle)
            .replace(/{replyDisplay}/g, canComment ? 'block' : 'none')
            .replace(/{toggleDisplay}/g, hasReplies ? 'inline-block' : 'none')
            .replace(/{replyCount}/g, comment.replyCount || 0);
    }
    
    function renderReplyHtml(comment, template) {
        const createdDate = new Date(comment.createdAt).toLocaleString();
        
        // Only allow editing if the user is logged in (currentUserId is not empty) and owns the comment
        const isLoggedIn = currentUserId && currentUserId.trim() !== "";
        const isOwner = isLoggedIn && String(currentUserId) === String(comment.userId);
        
        // Log information for debugging
        console.log(`Reply ${comment.id} by user ${comment.userId}, current user: ${currentUserId}, isLoggedIn: ${isLoggedIn}, isOwner: ${isOwner}`);
        
        // Completely hide buttons if not the owner
        const displayStyle = isOwner ? 'flex' : 'none';
        
        return template
            .replace(/{id}/g, comment.id)
            .replace(/{userId}/g, comment.userId)
            .replace(/{content}/g, comment.content)
            .replace(/{userName}/g, comment.userName)
            .replace(/{userProfilePicture}/g, comment.userProfilePicture || '/images/default-profile.jpg')
            .replace(/{createdAt}/g, createdDate)
            .replace(/{actionsDisplay}/g, displayStyle);
    }
    
    function toggleReplies(threadId, forceCollapse = false) {
        const repliesContainer = $(`#replies-${threadId}`);
        const collapsedIndicator = $(`#collapsed-${threadId}`);
        const toggleButton = $(`.toggle-replies[data-thread-id="${threadId}"]`);
        const toggleIcon = toggleButton.find('.toggle-icon');
        const toggleText = toggleButton.find('.toggle-text');
        
        // Get the number of replies
        const replyCount = parseInt(toggleButton.find('.reply-count').text());
        
        if (forceCollapse || repliesContainer.is(':visible')) {
            // Collapse replies
            repliesContainer.hide();
            collapsedIndicator.show();
            toggleIcon.removeClass('bi-dash-circle').addClass('bi-plus-circle');
            toggleText.text('Show replies');
        } else {
            // Expand replies
            repliesContainer.show();
            collapsedIndicator.hide();
            toggleIcon.removeClass('bi-plus-circle').addClass('bi-dash-circle');
            toggleText.text('Hide replies');
        }
    }
    
    function submitComment() {
        const content = $("#commentContent").val().trim();
        
        if (!content) {
            return;
        }
        
        // Show loading indicator and disable the button
        const submitBtn = $("#submitCommentBtn");
        const originalBtnText = submitBtn.html();
        submitBtn.prop("disabled", true).html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Posting...');
        
        const comment = {
            content: content,
            articleId: articleId
        };
        
        console.log("Submitting comment:", comment);
        
        $.ajax({
            url: "/Comments",
            method: "POST",
            contentType: "application/json",
            data: JSON.stringify(comment),
            headers: {
                "RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val()
            },
            success: function(response) {
                console.log("Comment posted successfully:", response);
                // Clear the comment input and hide actions
                $("#commentContent").val("").trigger("input");
                $(".comment-actions").slideUp();
                
                // The new comment will be added via SignalR
                // If SignalR fails for some reason, manually add the comment to the UI
                setTimeout(() => {
                    if (!commentsCache.some(c => c.id === response.id)) {
                        appendNewComment(response);
                    }
                }, 2000);
            },
            error: function(xhr) {
                console.error("Error adding comment:", xhr);
                console.log("Status:", xhr.status);
                console.log("Response:", xhr.responseText);
                
                let errorMessage = "Failed to add comment. Please try again.";
                
                if (xhr.responseJSON && xhr.responseJSON.message) {
                    errorMessage = xhr.responseJSON.message;
                }
                
                alert(errorMessage);
            },
            complete: function() {
                // Re-enable the button and restore its text
                submitBtn.prop("disabled", false).html(originalBtnText);
            }
        });
    }
    
    function appendNewComment(comment) {
        // Add to cache
        commentsCache.push(comment);
        
        // Update total comment count
        const totalComments = commentsCache.length;
        $("#totalCommentCount").text(totalComments);
        
        // Hide "no comments" message if shown
        $("#noComments").hide();
        
        // Check if this is a reply to another comment
        if (comment.parentCommentId) {
            const parentId = comment.parentCommentId;
            const parent = commentsCache.find(c => c.id === parentId);
            
            // If the parent doesn't exist in our cache, ignore this reply
            if (!parent) {
                console.error("Parent comment not found for reply:", comment);
                return;
            }
            
            // Get all replies for this parent
            const replies = commentsCache.filter(c => c.parentCommentId === parentId);
            
            // Update the reply count
            const toggleButton = $(`.toggle-replies[data-thread-id="${parentId}"]`);
            const replyCountEl = toggleButton.find('.reply-count');
            const collapsedReplyCountEl = $(`#collapsed-${parentId}`).find('.reply-count');
            
            replyCountEl.text(replies.length);
            collapsedReplyCountEl.text(replies.length);
            
            // Show the toggle button if it was hidden
            toggleButton.css('display', 'inline-block');
            
            // Re-render all replies to ensure proper formatting
            renderReplies(parentId, replies);
            
            // Make sure replies are visible
            toggleReplies(parentId, false);
            
            // Remove any existing reply form
            $(`#reply-form-${parentId}`).remove();
        } else {
            // This is a new parent comment
            comment.replyCount = 0; // No replies yet
            const parentTemplate = document.getElementById("commentTemplate").innerHTML;
            const commentHtml = renderCommentHtml(comment, parentTemplate, false);
            $("#commentsList").prepend(commentHtml);
        }
    }
    
    function showReplyForm(parentCommentId) {
        // Remove any existing reply forms
        $(".reply-form").remove();
        
        // Create a new reply form using the same style as the main comment box
        const replyForm = `
            <div class="reply-form mt-2 mb-3" id="reply-form-${parentCommentId}">
                <div class="d-flex">
                    <div class="flex-shrink-0">
                        <img src="${currentUserProfilePicture}" 
                             alt="Your profile" 
                             class="rounded-circle me-3" 
                             width="32" 
                             height="32"
                             >
                    </div>
                    <div class="flex-grow-1">
                        <form>
                            <input type="hidden" class="parent-comment-id" value="${parentCommentId}">
                            <div class="mb-2">
                                <textarea class="form-control border-0 comment-input reply-content" 
                                          rows="1" 
                                          placeholder="Add a reply..."></textarea>
                            </div>
                            <div class="comment-actions" style="display: flex;">
                                <button type="button" class="btn btn-light cancel-reply-btn">Cancel</button>
                                <button type="submit" class="btn btn-primary submit-reply-btn" disabled>Reply</button>
                            </div>
                        </form>
                    </div>
                </div>
            </div>
        `;
        
        // Insert the form after the comment
        $(`#comment-${parentCommentId}`).after(replyForm);
        
        // Focus the textarea and handle auto-expand
        const textarea = $(`#reply-form-${parentCommentId} textarea`);
        textarea.focus();
        
        // Handle textarea input for auto-expand and button enable/disable
        textarea.on("input", function() {
            const content = $(this).val().trim();
            $(this).closest('form').find('.submit-reply-btn').prop('disabled', !content);
            
            // Auto-expand textarea
            this.style.height = "auto";
            this.style.height = (this.scrollHeight) + "px";
        });
        
        // Handle cancel button
        $(".cancel-reply-btn").on("click", function() {
            $(this).closest(".reply-form").remove();
        });
        
        // Handle form submission
        $(".submit-reply-btn").closest("form").on("submit", function(e) {
            e.preventDefault();
            const form = $(this);
            submitReply(form);
        });
        
        // Make sure replies are visible
        toggleReplies(parentCommentId, false);
    }
    
    function openEditModal(commentId) {
        const comment = commentsCache.find(c => c.id === commentId);
        if (!comment) return;
        
        // Remove any existing edit forms
        $(".edit-form").remove();
        
        // Create inline edit form
        const editForm = `
            <div class="edit-form mt-2 mb-3" id="edit-form-${commentId}">
                <div class="d-flex">
                    <div class="flex-shrink-0">
                        <img src="${currentUserProfilePicture}" 
                             alt="Your profile" 
                             class="rounded-circle me-3" 
                             width="32" 
                             height="32"
                             onerror="this.src='/images/default-profile.png';">
                    </div>
                    <div class="flex-grow-1">
                        <form>
                            <input type="hidden" class="edit-comment-id" value="${commentId}">
                            <div class="mb-2">
                                <textarea class="form-control border-0 comment-input edit-content" 
                                          rows="1" 
                                          placeholder="Edit your comment...">${comment.content}</textarea>
                            </div>
                            <div class="comment-actions" style="display: flex;">
                                <button type="button" class="btn btn-light cancel-edit-btn">Cancel</button>
                                <button type="submit" class="btn btn-primary save-edit-btn">Save</button>
                            </div>
                        </form>
                    </div>
                </div>
            </div>
        `;
        
        // Hide the original comment content and add edit form
        const commentElement = $(`#comment-${commentId}`);
        commentElement.find('.comment-content').hide();
        commentElement.find('.comment-meta').after(editForm);
        
        // Focus the textarea and handle auto-expand
        const textarea = $(`#edit-form-${commentId} textarea`);
        textarea.focus();
        
        // Handle textarea input for auto-expand
        textarea.on("input", function() {
            // Auto-expand textarea
            this.style.height = "auto";
            this.style.height = (this.scrollHeight) + "px";
        });
        
        // Trigger input event to set initial height
        textarea.trigger("input");
        
        // Handle cancel button
        $(".cancel-edit-btn").on("click", function() {
            const form = $(this).closest(".edit-form");
            const commentId = form.find(".edit-comment-id").val();
            $(`#comment-${commentId}`).find('.comment-content').show();
            form.remove();
        });
        
        // Handle form submission
        $(".save-edit-btn").closest("form").on("submit", function(e) {
            e.preventDefault();
            const form = $(this);
            const commentId = form.find(".edit-comment-id").val();
            const content = form.find(".edit-content").val().trim();
            
            if (!content) return;
            
            // Show loading indicator and disable the button
            const submitBtn = form.find(".save-edit-btn");
            const originalBtnText = submitBtn.html();
            submitBtn.prop("disabled", true).html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Saving...');
            
            const comment = {
                id: parseInt(commentId),
                content: content
            };
            
            $.ajax({
                url: `/Comments/${commentId}`,
                method: "PUT",
                contentType: "application/json",
                data: JSON.stringify(comment),
                headers: {
                    "RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val()
                },
                success: function(response) {
                    console.log("Comment updated successfully:", response);
                    
                    // The comment will be updated via SignalR
                    // Show success message
                    const successAlert = $('<div>')
                        .addClass('alert alert-success fade show')
                        .css({
                            'position': 'fixed',
                            'top': '20px',
                            'right': '20px',
                            'z-index': 1050,
                            'max-width': '300px'
                        })
                        .html(`
                            <div class="d-flex align-items-center">
                                <i class="bi bi-check-circle me-2"></i>
                                Comment updated successfully
                            </div>
                        `);
                    
                    $('body').append(successAlert);
                    
                    // Remove the success message after 3 seconds
                    setTimeout(() => {
                        successAlert.fadeOut('slow', function() {
                            $(this).remove();
                        });
                    }, 3000);
                    
                    // Clean up form
                    const commentElement = $(`#comment-${commentId}`);
                    commentElement.find('.comment-content').show();
                    form.closest(".edit-form").remove();
                    
                    // If SignalR fails, update the comment after 2 seconds
                    setTimeout(() => {
                        const cachedComment = commentsCache.find(c => c.id === parseInt(commentId));
                        if (cachedComment && cachedComment.content !== content) {
                            updateExistingComment({...cachedComment, content: content});
                        }
                    }, 2000);
                },
                error: function(xhr) {
                    console.error("Error updating comment:", xhr);
                    console.log("Status:", xhr.status);
                    console.log("Response:", xhr.responseText);
                    
                    let errorMessage = "Failed to update comment. Please try again.";
                    
                    if (xhr.responseJSON && xhr.responseJSON.message) {
                        errorMessage = xhr.responseJSON.message;
                    }
                    
                    // Show error message in a toast
                    const errorAlert = $('<div>')
                        .addClass('alert alert-danger fade show')
                        .css({
                            'position': 'fixed',
                            'top': '20px',
                            'right': '20px',
                            'z-index': 1050,
                            'max-width': '300px'
                        })
                        .html(`
                            <div class="d-flex align-items-center">
                                <i class="bi bi-exclamation-circle me-2"></i>
                                ${errorMessage}
                            </div>
                        `);
                    
                    $('body').append(errorAlert);
                    
                    // Remove the error message after 5 seconds
                    setTimeout(() => {
                        errorAlert.fadeOut('slow', function() {
                            $(this).remove();
                        });
                    }, 5000);
                },
                complete: function() {
                    // Re-enable the button and restore its text
                    submitBtn.prop("disabled", false).html(originalBtnText);
                }
            });
        });
    }
    
    function updateExistingComment(comment) {
        // Update in cache
        const index = commentsCache.findIndex(c => c.id === comment.id);
        if (index !== -1) {
            commentsCache[index] = comment;
        }
        
        // Update in DOM - either in the main list or in a replies container
        const commentElement = $(`#comment-${comment.id}`);
        if (commentElement.length > 0) {
            commentElement.find(".comment-content").text(comment.content);
        }
    }
    
    function deleteComment(commentId) {
        if (!confirm("Are you sure you want to delete this comment?")) {
            return;
        }
        
        // Show loading state
        const deleteBtn = $(`.delete-comment-btn[data-id="${commentId}"]`);
        const originalBtnHtml = deleteBtn.html();
        deleteBtn.prop('disabled', true).html('<i class="bi bi-hourglass-split"></i> Deleting...');
        
        $.ajax({
            url: `/Comments/${commentId}`,
            method: "DELETE",
            headers: {
                "RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val()
            },
            success: function(response) {
                // The comment will be removed via SignalR
                // Show success message
                const commentElement = $(`#comment-${commentId}`);
                const successAlert = $('<div>')
                    .addClass('alert alert-success fade show')
                    .css({
                        'position': 'fixed',
                        'top': '20px',
                        'right': '20px',
                        'z-index': 1050,
                        'max-width': '300px'
                    })
                    .html(`
                        <div class="d-flex align-items-center">
                            <i class="bi bi-check-circle me-2"></i>
                            Comment deleted successfully
                        </div>
                    `);
                
                $('body').append(successAlert);
                
                // Remove the success message after 3 seconds
                setTimeout(() => {
                    successAlert.fadeOut('slow', function() {
                        $(this).remove();
                    });
                }, 3000);
                
                // If SignalR fails, remove the comment after 2 seconds
                setTimeout(() => {
                    if ($(`#comment-${commentId}`).length > 0) {
                        removeComment(commentId);
                    }
                }, 2000);
            },
            error: function(xhr) {
                console.error("Error deleting comment:", xhr);
                let errorMessage = "Failed to delete comment. Please try again.";
                
                if (xhr.responseJSON && xhr.responseJSON.message) {
                    errorMessage = xhr.responseJSON.message;
                }
                
                // Show error message in a toast
                const errorAlert = $('<div>')
                    .addClass('alert alert-danger fade show')
                    .css({
                        'position': 'fixed',
                        'top': '20px',
                        'right': '20px',
                        'z-index': 1050,
                        'max-width': '300px'
                    })
                    .html(`
                        <div class="d-flex align-items-center">
                            <i class="bi bi-exclamation-circle me-2"></i>
                            ${errorMessage}
                        </div>
                    `);
                
                $('body').append(errorAlert);
                
                // Remove the error message after 5 seconds
                setTimeout(() => {
                    errorAlert.fadeOut('slow', function() {
                        $(this).remove();
                    });
                }, 5000);
            },
            complete: function() {
                // Restore the delete button
                deleteBtn.prop('disabled', false).html(originalBtnHtml);
                // Close the kebab menu
                deleteBtn.closest('.kebab-menu-content').removeClass('show');
            }
        });
    }
    
    function removeComment(commentId) {
        console.log(`Removing comment ID: ${commentId}`);
        
        // Find the comment to be removed
        const removedComment = commentsCache.find(c => c.id === commentId);
        if (!removedComment) {
            console.warn(`Comment with ID ${commentId} not found in cache, cannot remove`);
            return;
        }
        
        // Remove from cache
        commentsCache = commentsCache.filter(c => c.id !== commentId);
        
        // Check if this is a parent comment
        const isParent = !removedComment.parentCommentId;
        
        if (isParent) {
            console.log(`Removing parent comment ID: ${commentId} and all its replies`);
            // Find all replies to this parent comment
            const replies = commentsCache.filter(c => c.parentCommentId === commentId);
            console.log(`Found ${replies.length} replies to remove:`, replies);
            
            // Remove all replies from cache
            commentsCache = commentsCache.filter(c => c.parentCommentId !== commentId);
        } else {
            console.log(`Removing reply comment ID: ${commentId}`);
        }
        
        // Update total comment count
        const totalComments = commentsCache.length;
        $("#totalCommentCount").text(totalComments);
        
        if (isParent) {
            // This is a parent comment - remove the entire thread with animation
            console.log(`Removing thread-${commentId} from DOM`);
            $(`#thread-${commentId}`).fadeOut('fast', function() {
                $(this).remove();
                
                // Show "no comments" if no comments left
                if (commentsCache.length === 0) {
                    $("#noComments").fadeIn();
                }
            });
        } else {
            // This is a reply - remove just this comment
            console.log(`Removing reply comment-${commentId} from DOM`);
            $(`#comment-${commentId}`).fadeOut('fast', function() {
                $(this).remove();
                
                // Update parent comment's reply count and UI
                const parentId = removedComment.parentCommentId;
                const repliesContainer = $(`#replies-${parentId}`);
                const remainingReplies = commentsCache.filter(c => c.parentCommentId === parentId);
                
                console.log(`Parent ${parentId} now has ${remainingReplies.length} replies remaining`);
                
                if (remainingReplies.length === 0) {
                    // No more replies, hide the toggle button and container
                    console.log(`No more replies for parent ${parentId}, hiding toggle button`);
                    $(`.toggle-replies[data-thread-id="${parentId}"]`).hide();
                    repliesContainer.empty();
                } else {
                    // Update reply count
                    console.log(`Updating reply count for parent ${parentId} to ${remainingReplies.length}`);
                    $(`.toggle-replies[data-thread-id="${parentId}"] .reply-count`).text(remainingReplies.length);
                    $(`#collapsed-${parentId} .reply-count`).text(remainingReplies.length);
                }
            });
        }
    }
    
    function submitReply(form) {
        const parentCommentId = form.find(".parent-comment-id").val();
        const content = form.find(".reply-content").val().trim();
        
        if (!content) {
            return;
        }
        
        // Show loading indicator and disable the button
        const submitBtn = form.find(".submit-reply-btn");
        const originalBtnText = submitBtn.html();
        submitBtn.prop("disabled", true).html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Posting...');
        
        const comment = {
            content: content,
            articleId: articleId,
            parentCommentId: parseInt(parentCommentId)
        };
        
        console.log("Submitting reply:", comment);
        
        $.ajax({
            url: "/Comments",
            method: "POST",
            contentType: "application/json",
            data: JSON.stringify(comment),
            headers: {
                "RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val()
            },
            success: function(response) {
                console.log("Reply posted successfully:", response);
                
                // The new reply will be added via SignalR
                // Show success message
                const successAlert = $('<div>')
                    .addClass('alert alert-success fade show')
                    .css({
                        'position': 'fixed',
                        'top': '20px',
                        'right': '20px',
                        'z-index': 1050,
                        'max-width': '300px'
                    })
                    .html(`
                        <div class="d-flex align-items-center">
                            <i class="bi bi-check-circle me-2"></i>
                            Reply posted successfully
                        </div>
                    `);
                
                $('body').append(successAlert);
                
                // Remove the success message after 3 seconds
                setTimeout(() => {
                    successAlert.fadeOut('slow', function() {
                        $(this).remove();
                    });
                }, 3000);
                
                // Clean up form
                form.closest(".reply-form").remove();
                
                // If SignalR fails, add the reply after 2 seconds
                setTimeout(() => {
                    if (!commentsCache.some(c => c.id === response.id)) {
                        appendNewComment(response);
                    }
                }, 2000);
            },
            error: function(xhr) {
                console.error("Error adding reply:", xhr);
                console.log("Status:", xhr.status);
                console.log("Response:", xhr.responseText);
                
                let errorMessage = "Failed to add reply. Please try again.";
                
                if (xhr.responseJSON && xhr.responseJSON.message) {
                    errorMessage = xhr.responseJSON.message;
                }
                
                // Show error message in a toast
                const errorAlert = $('<div>')
                    .addClass('alert alert-danger fade show')
                    .css({
                        'position': 'fixed',
                        'top': '20px',
                        'right': '20px',
                        'z-index': 1050,
                        'max-width': '300px'
                    })
                    .html(`
                        <div class="d-flex align-items-center">
                            <i class="bi bi-exclamation-circle me-2"></i>
                            ${errorMessage}
                        </div>
                    `);
                
                $('body').append(errorAlert);
                
                // Remove the error message after 5 seconds
                setTimeout(() => {
                    errorAlert.fadeOut('slow', function() {
                        $(this).remove();
                    });
                }, 5000);
            },
            complete: function() {
                // Re-enable the button and restore its text
                submitBtn.prop("disabled", false).html(originalBtnText);
            }
        });
    }
    
    function renderReplies(parentCommentId, replies) {
        const repliesContainer = $(`#replies-${parentCommentId}`);
        repliesContainer.empty();
        
        if (replies.length === 0) {
            return;
        }
        
        // Add a visual container for replies with indent
        const repliesWrapper = $('<div class="replies mt-2"></div>');
        
        // Sort replies by creation date
        replies.sort((a, b) => new Date(a.createdAt) - new Date(b.createdAt));
        
        // Render each reply
        replies.forEach(reply => {
            const replyTemplate = $('#replyTemplate').html();
            const replyHtml = renderReplyHtml(reply, replyTemplate);
            repliesWrapper.append(replyHtml);
        });
        
        repliesContainer.append(repliesWrapper);
    }
    
    // Clean up SignalR connection when leaving page
    $(window).on("beforeunload", function() {
        if (connection) {
            connection.invoke("LeaveArticleGroup", articleId);
            connection.stop();
        }
    });
} 