// تم تصحيح روابط مكتبات Firebase هنا
importScripts('https://www.gstatic.com/firebasejs/10.12.3/firebase-app.js');
importScripts('https://www.gstatic.com/firebasejs/10.12.3/firebase-messaging.js');

const firebaseConfig = {
    apiKey: "AIzaSyDhjBdgvyPh3SUSj8q-FxeneZxqnqYakxk",
    authDomain: "saqyaapp.firebaseapp.com",
    projectId: "saqyaapp",
    storageBucket: "saqyaapp.firebasestorage.app",
    messagingSenderId: "686762686266",
    appId: "1:686762686266:web:4d7202aa3b982781664203",
    measurementId: "G-HP5LEXXSNT"
};

const app = firebase.initializeApp(firebaseConfig);
const messaging = firebase.messaging();