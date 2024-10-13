<!-- src/components/ChatInterface.vue -->
<template>
    <div class="chat-container">
      <div class="messages" ref="messagesContainer">
        <div
          v-for="(msg, index) in messages"
          :key="index"
          :class="{
            'user-message': msg.sender === 'user',
            'bot-message': msg.sender === 'bot',
            'error-message': msg.sender === 'error',
          }"
        >
          <p>{{ msg.text }}</p>
          <span v-if="msg.citation" class="citation">Source: {{ msg.citation }}</span>
        </div>
        <div v-if="isLoading" class="loading-indicator">
          <span class="spinner"></span> Processing...
        </div>
      </div>
      <form @submit.prevent="sendMessage" class="input-area">
        <input
          type="text"
          v-model="input"
          placeholder="Type your message..."
          required
          :disabled="isLoading"
        />
        <button type="submit" :disabled="isLoading">Send</button>
      </form>
    </div>
  </template>
  
  <script lang="ts">
  import { defineComponent, ref, onMounted, nextTick } from 'vue';
  import axios from 'axios';
  import { ChatMessage } from '../types/ChatMessage';
  import { ChatResponse } from '../types/ChatResponse';
  
  interface Message {
    sender: 'user' | 'bot' | 'error';
    text: string;
    citation?: string;
  }
  
  export default defineComponent({
    name: 'ChatInterface',
    setup() {
      const messages = ref<Message[]>([]);
      const input = ref('');
      const isLoading = ref(false);
      const messagesContainer = ref<HTMLElement | null>(null);
  
      const apiUrl = process.env.VUE_APP_BACKEND_URL || 'http://localhost:5000/api/Chat/chatCompletion';
  
      const sendMessage = async () => {
        const userInput = input.value.trim();
        if (!userInput) return;
  
        // Add user message to chat
        messages.value.push({ sender: 'user', text: userInput });
  
        // Prepare chat messages for backend (maintaining conversation context)
        const chatMessages: ChatMessage[] = messages.value
          .filter(msg => msg.sender === 'user' || msg.sender === 'bot')
          .map(msg => ({
            Role: msg.sender === 'user' ? 'user' : 'assistant',
            Content: msg.text,
          }));
  
        isLoading.value = true;
  
        try {
            const response = await axios.post<ChatResponse | string>(apiUrl, chatMessages, {
                    headers: {
                        'Content-Type': 'application/json',
                    },
                });

                // Initialize variables to hold reply and citation
                let replyText: string = 'No reply';
                let citationText: string | undefined = undefined;

                // Check if response.data is an object and has the 'reply' property
                if (typeof response.data === 'object' && 'reply' in response.data) {
                    const chatResponse = response.data as ChatResponse;
                    replyText = chatResponse.reply;
                    citationText = chatResponse.citation;
                } else if (typeof response.data === 'string') {
                    // If response.data is a string, use it directly as the reply
                    replyText = response.data;
                }

                // Add bot reply to chat
                messages.value.push({
                sender: 'bot',
                text: replyText,
                citation: citationText,
                });


        } catch (error: any) {
          console.error('Error communicating with backend:', error);
  
          // Determine error message
          let errorMessage = 'Sorry, there was an error processing your request.';
          if (axios.isAxiosError(error)) {
            if (error.response) {
              errorMessage += ` (${error.response.status}: ${error.response.data})`;
            } else if (error.request) {
              errorMessage += ' No response received from the server.';
            } else {
              errorMessage += ` ${error.message}`;
            }
          }
  
          // Add error message to chat
          messages.value.push({
            sender: 'error',
            text: errorMessage,
          });
        } finally {
          input.value = '';
          isLoading.value = false;
          await nextTick();
          scrollToBottom();
        }
      };
  
      const scrollToBottom = () => {
        if (messagesContainer.value) {
          messagesContainer.value.scrollTop = messagesContainer.value.scrollHeight;
        }
      };
  
      onMounted(() => {
        scrollToBottom();
      });
  
      return {
        messages,
        input,
        sendMessage,
        isLoading,
        messagesContainer,
      };
    },
  });
  </script>
  
  <style scoped>
  .chat-container {
    display: flex;
    flex-direction: column;
    height: 100vh;
    background-color: #000;
    color: #fff;
    padding: 20px;
    box-sizing: border-box;
  }
  
  .messages {
    flex: 1;
    overflow-y: auto;
    margin-bottom: 10px;
    padding-right: 10px;
  }
  
  .user-message {
    align-self: flex-end;
    background-color: #1a73e8;
    padding: 10px;
    border-radius: 10px;
    margin: 5px 0;
    max-width: 70%;
    word-wrap: break-word;
  }
  
  .bot-message {
    align-self: flex-start;
    background-color: #333;
    padding: 10px;
    border-radius: 10px;
    margin: 5px 0;
    max-width: 70%;
    word-wrap: break-word;
  }
  
  .error-message {
    align-self: flex-start;
    background-color: #ff4d4f;
    padding: 10px;
    border-radius: 10px;
    margin: 5px 0;
    max-width: 70%;
    word-wrap: break-word;
    color: #fff;
  }
  
  .citation {
    display: block;
    font-size: 0.8em;
    color: #bbb;
    margin-top: 5px;
  }
  
  .input-area {
    display: flex;
    border-top: 1px solid #555;
    padding-top: 10px;
  }
  
  .input-area input {
    flex: 1;
    padding: 10px;
    border: none;
    border-radius: 5px;
    margin-right: 10px;
    background-color: #222;
    color: #fff;
    font-size: 1em;
  }
  
  .input-area input::placeholder {
    color: #888;
  }
  
  .input-area button {
    padding: 10px 20px;
    border: none;
    border-radius: 5px;
    background-color: #1a73e8;
    color: #fff;
    cursor: pointer;
    font-size: 1em;
    transition: background-color 0.3s ease;
  }
  
  .input-area button:disabled {
    background-color: #555;
    cursor: not-allowed;
  }
  
  .input-area button:hover:not(:disabled) {
    background-color: #1558b0;
  }
  
  .loading-indicator {
    display: flex;
    align-items: center;
    color: #bbb;
    font-size: 0.9em;
    margin-bottom: 10px;
  }
  
  .spinner {
    border: 4px solid #f3f3f3;
    border-top: 4px solid #1a73e8;
    border-radius: 50%;
    width: 16px;
    height: 16px;
    animation: spin 1s linear infinite;
    margin-right: 8px;
  }
  
  @keyframes spin {
    to {
      transform: rotate(360deg);
    }
  }
  </style>
  