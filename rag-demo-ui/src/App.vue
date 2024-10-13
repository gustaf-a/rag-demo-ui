<template>
  <div id="app">
    <ChatInterface />
  </div>
</template>

<script>
import ChatInterface from './components/ChatInterface.vue';

export default {
  name: 'App',
  components: {
    ChatInterface,
  },
};
</script>

<style>
/* Ensure the app takes full height */
#app {
  height: 100vh;
  margin: 0;
}
</style>